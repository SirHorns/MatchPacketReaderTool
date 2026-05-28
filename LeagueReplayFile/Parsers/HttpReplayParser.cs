using System.IO.Compression;
using System.Text;
using LeagueReplayFile.Encryption;
using LeagueReplayFile.Enums;
using LeagueReplayFile.Models;
using LeagueReplayFile.Models.Http;
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
    public RequestType CurrentRequestType { get; private set; }

    public HttpReplayParser(byte[] encryptionKey, long matchId)
    {
        var checksumKey = Encoding.ASCII.GetBytes(matchId.ToString());
        var checksumBlowfish = new BlowFish(checksumKey);
        var realKey = checksumBlowfish.Decrypt(encryptionKey);
        _blowfish = new BlowFish(realKey.Take(16).ToArray());
        CurrentRequestType = RequestType.NONE;
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

    //<•······················•<>•······················•>
    
    protected override void OnGetBinary(byte[] data)
    {
        var decompressed = _blowfish.DecompressToMemoryStream(data);
        using var reader = new BinaryReader(decompressed) ;
        var pkts = new List<ENetPacket>();
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var packet = ENetPacket.Create(reader);
            pkts.Add(packet);
        }
        
        switch (SectionBuffer)
        {
            case GameDataChunkSection gameDataSection:
                gameDataSection.PacketData = data;
                gameDataSection.Chunk.Packets.AddRange(pkts);
                break;
            case KeyFrameSection keyFrameSection:
                keyFrameSection.PacketData = data;
                keyFrameSection.Packets.AddRange(pkts);
                break;
        }
        
        Sections.Add(SectionBuffer);
    }
    
    protected override void OnGetText(byte[] data)
    {
        switch (CurrentRequestType)
        {
            case RequestType.END_OF_GAME_STATS:
            case RequestType.KEY_FRAME:
            case RequestType.GAME_DATA_CHUNK:
            case RequestType.NONE:
                Console.WriteLine($"Attempted to get text from non-text section!: {CurrentRequestType}");
                return;
        }

        var json = Encoding.UTF8.GetString(data);
        switch (CurrentRequestType)
        {
            case RequestType.VERSION:
            case RequestType.GAME_META_DATA:
            case RequestType.LAST_CHUNK_INFO:
                (SectionBuffer as IJsonSection)?.SetValues(json);
                break;
        }

        // probbly a better way to do this
        // but most non data chunks so far are at most a little over 800 bytes
        /*if (data.Length > 900)
        {
            OnGetBinary(data);
        }*/
    }
    
    /// <summary>
    /// Reads the next HTTP request
    /// </summary>
    /// <param name="data"></param>
    /// <param name="time"></param>
    /// <param name="segment"></param>
    protected override void OnDone(byte[] data, float time, DataSegment segment)
    {
        SectionBuffer = new Section()
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

        SectionBuffer.Http = replayReq;
        SectionBuffer.Data = data;
        SectionBuffer.Time = time;
        Console.WriteLine($"[HTTP]: {httpReq} {replayReq}");
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
        
        Sections.Add(SectionBuffer);
    }

    protected override void OnContinueBinary(byte[] data )
    {
        ByteBuffer.AddRange(data);
        if (ByteBuffer.Count > ExpectedLengthBuffer)
        {
            throw new IOException("Buffer overrun!");
        }

        if (ByteBuffer.Count != ExpectedLengthBuffer)
        {
            return;
        }

        OnGetBinary(ByteBuffer.ToArray());
        SetHttpState(HttpState.Done);
        Sections.Add(SectionBuffer);
        ByteBuffer.Clear();
        ExpectedLengthBuffer = 0;
    }
    
    protected override void OnContinueText(byte[] data)
    {
        ByteBuffer.AddRange(data);
        if (ByteBuffer.Count > ExpectedLengthBuffer)
        {
            throw new IOException("Buffer overrun!");
        }

        if (ByteBuffer.Count != ExpectedLengthBuffer)
        {
            return;
        }

        OnGetText(ByteBuffer.ToArray());
        SetHttpState(HttpState.Done);
        Sections.Add(SectionBuffer);
        ByteBuffer.Clear();
        ExpectedLengthBuffer = 0;
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
                ByteBuffer.AddRange(content);
                ExpectedLengthBuffer = contentLength;
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
                ByteBuffer.AddRange(content);
                ExpectedLengthBuffer = contentLength;
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
            Sections.Add(SectionBuffer);
            return;
        }

        var api = request.Split("/");
        var http = SectionBuffer.Http.Split("/");
        switch (api[4])
        {
            case "version":
                SetCurrentRequest(RequestType.VERSION);
                SetHttpState(HttpState.GetText);
                SectionBuffer = new VersionSection().Copy(SectionBuffer, type: RequestType.VERSION);
                break;
            case "getGameMetaData":
                SetCurrentRequest(RequestType.GAME_META_DATA);
                SetHttpState(HttpState.GetText);
                SectionBuffer = new GameMetaDataSection().Copy(SectionBuffer, type: RequestType.GAME_META_DATA);
                break;
            case "getLastChunkInfo":    // /RestServicePath + /consumer/getGameDataChunk + /platformID + /gameID + /minTimeAvailable + /AccessToken
                SetCurrentRequest(RequestType.LAST_CHUNK_INFO);
                SetHttpState(HttpState.GetText);
                SectionBuffer = new LastChunkInfoSection()
                {
                    MinTimeAvailable = int.Parse(http[^2])
                }.Copy(SectionBuffer, type: RequestType.LAST_CHUNK_INFO);
                break;
            case "getKeyFrame":         // /RestServicePath + /consumer/getKeyFrame + /platformID + /gameID + /chunkID + /AccessToken
                SetCurrentRequest(RequestType.KEY_FRAME);
                SetHttpState(HttpState.GetBinary);
                SectionBuffer = new KeyFrameSection()
                {
                    ID = int.Parse(http[^2])
                }.Copy(SectionBuffer, type: RequestType.KEY_FRAME);
                break;
            case "getGameDataChunk":    // /RestServicePath + /consumer/getGameDataChunk + /platformID +/gameID +/chunkID + /AccessToken
                SetCurrentRequest(RequestType.GAME_DATA_CHUNK);
                SetHttpState(HttpState.GetBinary);
                var id = int.Parse(http[^2]);
                var gameId = long.Parse(http[^3]);
                SectionBuffer = new GameDataChunkSection
                {
                    ID = id,
                    GameId = gameId,
                    Chunk = new Chunk()
                    {
                        ID = id
                    }
                }.Copy(SectionBuffer, type: RequestType.GAME_DATA_CHUNK);
                break;
            case "end":                 // /RestServicePath + /consumer/end + unknown-args
                throw new NotImplementedException("end (OfGameStats) is not implemented yet");
            default:
                SectionBuffer.Type = RequestType.NONE;
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

    private void SetCurrentRequest(RequestType type)
    {
        CurrentRequestType = type;
    }
}