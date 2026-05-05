using System.IO.Compression;
using System.Text;
using LeagueReplayFile.Enums;
using LeagueReplayFile.Models;
using LeagueReplayFile.Models.Sections;
using LeagueReplayFile.Protocols;
using LeagueReplayFile.Protocols.ENet;

namespace LeagueReplayFile.Parsers;

/// <summary>
/// Parses replays that were sent over HTTP
/// </summary>
public class HttpReplayParser : HttpProtocol, ILRFParser
{
    private readonly BlowFish _blowfish;
    private BinaryReader _reader;
    public List<DataSegment> Segments { get; }
    public List<ENetPacket> Packets { get; }
    public List<Section> Sections { get; }
    public RequestTypes CurrentRequest { get; private set; }

    public HttpReplayParser(byte[] encryptionKey, long matchId)
    {
        var checksumKey = Encoding.ASCII.GetBytes(matchId.ToString());
        var checksumBlowfish = new BlowFish(checksumKey);
        var realKey = checksumBlowfish.Decrypt(encryptionKey);
        _blowfish = new BlowFish(realKey.Take(16).ToArray());
        CurrentRequest = RequestTypes.NONE;
        Sections = [];
        Packets = [];
        Segments = [];
    }

    public void Read(byte[] data)
    {
        _reader = new BinaryReader(new MemoryStream(data));

        Console.WriteLine("Reading Segments...");
        while (_reader.BaseStream.Position < _reader.BaseStream.Length)
        {
            var segment = DataSegment.Read(_reader);
            Segments.Add(segment);
        }
        Console.WriteLine($"Found {Segments.Count} segments");
        Console.WriteLine($"Reading Segments...");
        for (var i = 0; i < Segments.Count; i++)
        {
            var segment = Segments[i];
            ParseSegment(segment);
        }
    }

    private MemoryStream Decompress(byte[] data)
    {
        var decrypted = _blowfish.Decrypt(data);
        var decompressed = new MemoryStream();
        using var compressed = new GZipStream(new MemoryStream(decrypted), CompressionMode.Decompress);
        compressed.CopyTo(decompressed);
        decompressed.Seek(0, SeekOrigin.Begin);
        return decompressed;
    }

    //<•······················•<>•······················•>
    
    protected override void OnGetBinary(byte[] data)
    {
        List<ENetPacket> pkts;
        var decompressed = Decompress(data);
        decompressed.Seek(0, SeekOrigin.Begin);
        using (var reader = new BinaryReader(decompressed))
        {
            pkts = ReadSectionPackets(reader);
        }

        switch (CurrentSection)
        {
            case GameDataSection gameDataSection:
                gameDataSection.Chunk.Packets.AddRange(pkts);
                break;
            case KeyFrameSection keyFrameSection:
                keyFrameSection.Packets.AddRange(pkts);
                break;
        }
        
        Sections.Add(CurrentSection);
    }
    
    protected override void OnGetText(byte[] data)
    {
        switch (CurrentRequest)
        {
            case RequestTypes.VERSION:
                ((VersionSection)CurrentSection).Text = Encoding.UTF8.GetString(data);
                break;
            case RequestTypes.GAME_META_DATA:
                ((GameMetaDataSection)CurrentSection).Json = Encoding.UTF8.GetString(data);
                break;
            case RequestTypes.LAST_CHUNK_INFO:
                ((LastChunkInfoSection)CurrentSection).Json = Encoding.UTF8.GetString(data);
                break;
            case RequestTypes.END_OF_GAME_STATS:
                break;
            case RequestTypes.KEY_FRAME:
            case RequestTypes.GAME_DATA_CHUNK:
            case RequestTypes.NONE:
            default:
                Console.WriteLine($"Attempted to get text from non-text section!: {CurrentRequest}");
                break;
        }

        // probbly a better way to do this
        // but most non data chunks so far are at most a little over 800 bytes
        if (data.Length > 900)
        {
            OnGetBinary(data);
        }
    }
    
    protected override void OnDone(byte[] data, float time, DataSegment segment)
    {
        CurrentSection = new Section()
        {
            Segment = segment,
        };

        var req = Encoding.UTF8.GetString(data).Split(' ');

        var httpReq = req[0];

        if (req.Length <= 1)
        {
            // weird http done req?
            // TODO: track janky requests
            return;
        }

        var replayReq = req[1];

        CurrentSection.Http = replayReq;
        CurrentSection.Data = data;
        CurrentSection.Time = time;

        switch (httpReq)
        {
            case "HTTP":
                Http(data);
                break;
            case "GET":
                Get(replayReq);
                break;
            case "POST":
                Post(replayReq);
                break;
            case "HEAD":
                Head(data, time);
                break;
            case "OPTIONS":
                Options(data, time);
                break;
            default:
                Console.WriteLine($"[BAD_REQ]: {httpReq}");
                break;
        }
        
        Sections.Add(CurrentSection);
    }

    protected override void OnContinueBinary(byte[] data )
    {
        Buffer.AddRange(data);
        if (Buffer.Count > BufferExpectedLength)
        {
            throw new IOException("Buffer overrun!");
        }

        if (Buffer.Count != BufferExpectedLength)
        {
            return;
        }

        OnGetBinary(Buffer.ToArray());
        SetHttpState(HttpState.Done);
        Sections.Add(CurrentSection);
        Buffer.Clear();
        BufferExpectedLength = 0;
    }
    
    protected override void OnContinueText(byte[] data)
    {
        Buffer.AddRange(data);
        if (Buffer.Count > BufferExpectedLength)
        {
            throw new IOException("Buffer overrun!");
        }

        if (Buffer.Count != BufferExpectedLength)
        {
            return;
        }

        OnGetText(Buffer.ToArray());
        SetHttpState(HttpState.Done);
        Sections.Add(CurrentSection);
        Buffer.Clear();
        BufferExpectedLength = 0;
    }
    
    //<•······················•<>•······················•>
    
    private void Http(byte[] data)
    {
        using var stream = new MemoryStream(data);
        var index = 0;
        var matchCount = 0;
        for (; index < data.Length && matchCount != 4; index++)
        {
            if (data[index] == HTTP_END[matchCount])
            {
                matchCount++;
            }
            else
            {
                matchCount = 0;
            }
        }

        if (matchCount != 4)
        {
            throw new IOException("Failed to find http end in stream!");
        }

        using var binary = new BinaryReader(stream, Encoding.UTF8, true);
        var http = Encoding.UTF8.GetString(binary.ReadExactBytes(index));
        var contentLengthMatch = RE_CONTENT_LEN.Match(http);
        if (!contentLengthMatch.Success)
        {
            return;
        }

        var contentLength = long.Parse(contentLengthMatch.Groups[1].Value);
        var content = binary.ReadExactBytes((int)binary.BytesLeft());


        if (!http.Contains("application/octet-stream"))
        {
            if (content.Length < contentLength)
            {
                Buffer.AddRange(content);
                BufferExpectedLength = contentLength;
                SetHttpState(HttpState.ContinueText);
            }
            else
            {
                OnGetText(content);
            }
        }
        else
        {
            if (content.Length < contentLength)
            {
                Buffer.AddRange(content);
                BufferExpectedLength = contentLength;
                SetHttpState(HttpState.ContinueBinary);
            }
            else
            {
                OnGetBinary(content);
            }
        }
    }
    
    private void Get(string request)
    {
        // /observer-mode/rest/consumer/<api-call>/
        if (request.Equals("\r\n"))
        {
            SetHttpState(HttpState.Done);
            Sections.Add(CurrentSection);
            return;
        }

        var api = request.Split("/");
        var http = CurrentSection.Http.Split("/");
        switch (api[4])
        {
            case "version":
                SetCurrentRequest(RequestTypes.VERSION);
                SetHttpState(HttpState.GetText);
                CurrentSection = new VersionSection()
                {
                }.Copy(CurrentSection, type: RequestTypes.VERSION);
                break;
            case "getGameMetaData":
                SetCurrentRequest(RequestTypes.GAME_META_DATA);
                SetHttpState(HttpState.GetText);
                CurrentSection = new GameMetaDataSection()
                {
                    GameId = int.Parse(http[^2])
                }.Copy(CurrentSection, type: RequestTypes.GAME_META_DATA);
                break;
            case "getLastChunkInfo":
                SetCurrentRequest(RequestTypes.LAST_CHUNK_INFO);
                SetHttpState(HttpState.GetText);
                CurrentSection = new LastChunkInfoSection()
                {
                    Unknown = int.Parse(http[^2])
                }.Copy(CurrentSection, type: RequestTypes.LAST_CHUNK_INFO);
                break;
            case "getKeyFrame":
                SetCurrentRequest(RequestTypes.KEY_FRAME);
                SetHttpState(HttpState.GetBinary);
                CurrentSection = new KeyFrameSection()
                {
                    ID = int.Parse(http[^2])
                }.Copy(CurrentSection, type: RequestTypes.KEY_FRAME);
                break;
            case "getGameDataChunk":
                SetCurrentRequest(RequestTypes.GAME_DATA_CHUNK);
                SetHttpState(HttpState.GetBinary);
                var id = int.Parse(http[^2]);
                var gameId = long.Parse(http[^3]);
                CurrentSection = new GameDataSection
                {
                    ID = id,
                    GameId = gameId,
                    Chunk = new GameDataChunk()
                    {
                        ID = id
                    }
                }.Copy(CurrentSection, type: RequestTypes.GAME_DATA_CHUNK);
                break;
            case "endOfGameStats":
                throw new NotImplementedException("endOfGameStats is not implemented yet");
            default:
                CurrentSection.Type = RequestTypes.NONE;
                Console.WriteLine(request);
                break;
        }
    }

    private void Post(string request)
    {
        Console.WriteLine(request);
        SetHttpState(HttpState.GetText);
    }

    private void Head(byte[] data, float time)
    {
        Console.WriteLine("HEAD");
    }

    private void Options(byte[] data, float time)
    {
        Console.WriteLine("HEAD");
    }
    
    //<•······················•<>•······················•>

    private void SetCurrentRequest(RequestTypes type)
    {
        CurrentRequest = type;
    }
    
    //<•······················•<>•······················•>
    
    private List<ENetPacket> ReadSectionPackets(BinaryReader reader)
    {
        var pkts = new List<ENetPacket>();
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var packet = ENetPacket.Read(reader);;
            pkts.Add(packet);
        }
        return pkts;
    }
}