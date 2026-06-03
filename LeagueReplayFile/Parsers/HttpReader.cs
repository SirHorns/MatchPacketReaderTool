using System.Text;
using LeagueReplayFile.Encryption;
using LeagueReplayFile.Enums;
using LeagueReplayFile.LRFs;
using LeagueReplayFile.Maestro;
using LeagueReplayFile.Models;
using LeagueReplayFile.Models.Http;
using LeagueReplayFile.Models.Sections;
using LeagueReplayFile.Protocols;
using LeagueReplayFile.Protocols.ENet;

namespace LeagueReplayFile.Parsers;

/// <summary>
/// Parses replays sent over HTTP
/// </summary>
public class HttpReader : HttpProtocol
{
    private readonly BlowFish _blowfish;
    private readonly ReplayRequest _currentRequestType;
    private Section? _sectionBuffer;
    private readonly HttpLRF _lrf;
    private int _index;
    private List<DataSegment> _segments;
    
    private List<byte> Buffer { get; }
    private long BufferExpectedLength { get; set; }
    
    public List<ENetPacket> Packets { get; }
    public List<Section> Sections { get; }
    
    public HttpReader(HttpLRF lrf)
    {
        _lrf = lrf;
        var encryptionKey = lrf.MetaData.EncryptionKey;
        var matchId = lrf.MetaData.MatchId;
        
        _currentRequestType = ReplayRequest.NONE;
        _sectionBuffer = null;
        
        var checksumKey = Encoding.ASCII.GetBytes(matchId.ToString());
        var checksumBlowfish = new BlowFish(checksumKey);
        var realKey = checksumBlowfish.Decrypt(encryptionKey);
        _blowfish = new BlowFish(realKey.Take(16).ToArray());
        
        Sections = [];
        Packets = [];
        Buffer = [];
    }
    

    public void ReadStreamData(byte[] streamBytes)
    {
        using var reader = new BinaryReader(new MemoryStream(streamBytes));

        if (_lrf.ReplayVersion > LRFReader.SpectatorVersion)
        {
            ReadStreamSpectatorId(reader);
        }
        else
        {
            ReadStream(reader);
        }
    }

    private void ReadStreamSpectatorId(BinaryReader reader)
    {
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var time = reader.ReadSingle();
            var length = reader.ReadInt32();
            Console.WriteLine($"Position: {reader.BaseStream.Position}  Reading: {length}");
            byte[] data;
            byte padding;
            if (length + reader.BaseStream.Position > reader.BaseStream.Length)
            {
                Console.Error.WriteLine("Replay tries to go past stored stream!!!");
                var remaining = reader.BaseStream.Length - reader.BaseStream.Position;
                data = new byte[remaining];
                for (int i = 0; i < remaining; i++)
                {
                    data[i] = reader.ReadByte();
                }
            }
            else
            {
                data = reader.ReadExactBytes(length);
                padding = reader.ReadByte();
            }
  
            if (length == 16)
            {
                var mm = MaestroMessage.Read(data);
                switch (mm.Type)
                {
                    case MessageType.CHATMESSAGE_TO_GAME:
                        break;
                    case MessageType.CHATMESSAGE_FROM_GAME:
                        /*var x = 0;
                        List<byte> t = [];
                        while (x != mm.DataLength)
                        {
                            time = reader.ReadSingle();
                            length = reader.ReadInt32();
                            x += length;
                            data = reader.ReadExactBytes(length); 
                            t.AddRange(data);
                            padding = reader.ReadByte();
                        }
                        mm.ExtraBytes = t.ToArray();
                        Console.WriteLine(Encoding.UTF8.GetString(mm.ExtraBytes));*/
                        //_read = true;
                        break;
                    default:
                        continue;
                }
                Console.WriteLine($"<{mm.Type}> :: <{mm.DataLength}>:: <{mm.Unknown2}>");
            }
            else
            {
                Console.WriteLine(Encoding.UTF8.GetString(data));
            }
        }
    }

    private void ReadStream(BinaryReader reader)
    {
        _segments = [];
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var segment = DataSegment.Read(reader);
            _segments.Add(segment);
        }

        while (_index < _segments.Count)
        { 
            ParseSegment(GetNextSegment());
        }
    }
    
    private void ReadMaestro(byte[] data)
    {
         switch (data.Length)
        {
            case 16:
                var message = MaestroMessage.Read(data);
                _lrf.Messages.Add(message);
                if (message.Type is  (MessageType.ACK or MessageType.HEARTBEAT))
                {
                    return;
                }
                Console.Write($"[MAESTRO]: {message.Type};");
                if (message.DataLength > 0)
                {
                    Console.WriteLine($" Next data is {message.DataLength} bytes long;");
                }
                else
                {
                    Console.WriteLine("");
                }
                switch (message.Type)
                {
                    case MessageType.GAME_START:
                        break;
                    case MessageType.GAME_END:
                        break;
                    case MessageType.GAME_CRASHED:
                        break;
                    case MessageType.CLOSE:
                        break;
                    case MessageType.HEARTBEAT:
                        break;
                    case MessageType.ACK:
                        return;
                    case MessageType.GAMECLIENT_CREATE:
                        break;
                    case MessageType.GAMECLIENT_ABANDONED:
                        break;
                    case MessageType.GAMECLIENT_LAUNCHED:
                        break;
                    case MessageType.GAMECLIENT_STOPPED:
                        break;
                    case MessageType.GAMECLIENT_CONNECTED_TO_SERVER:
                        break;
                    case MessageType.CHATMESSAGE_TO_GAME:
                    case MessageType.CHATMESSAGE_FROM_GAME:
                        break;
                    default:
                        break;
                }
                break;
            default:
                break;
        }
    }
    
    private DataSegment GetNextSegment()
    {
        var seg = _segments[_index];
        _index++;
        return seg;
    }

    private void ParseSegment(DataSegment segment)
    {
        var data = segment.Data;
        var time = segment.Time;
        var text = Encoding.UTF8.GetString(data);
        var req = text.Split(' ');
        var httpRequest = req[0];
        var api = req[1];

        _sectionBuffer = new Section()
        {
            Segment = segment,
            Data = data,
            Time = time,
            Http = httpRequest,
            Api = api
        };
        
        switch (httpRequest)
        {
            case "HTTP":
                Http(data);
                break;
            case "GET":
                Get(api);
                break;
            case "POST":
                Post(api);
                break;
            case "HEAD":
                Head(data, time);
                break;
            case "OPTIONS":
                Options(data, time);
                break;
            default:
                Console.WriteLine($"[BAD_REQ]: {httpRequest}");
                break;
        }

    }

    //<•······················•<>•······················•>
    
    protected override void Http(byte[] data)
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
            }
            else
            {
                OnGetBinary(content);
            }
        }
    }
    
    protected override void Get(string request)
    {
        // /observer-mode/rest/consumer/<api-call>/
        if (request.Equals("\r\n"))
        {
            Sections.Add(_sectionBuffer);
            return;
        }

        var api = request.Split("/");
        var replayRequest = api[4];
        string text = "";
        DataSegment nextSegment;

        // TEXT GET
        switch (replayRequest)
        {
            case "version":
                _sectionBuffer = new VersionSection().Copy(_sectionBuffer, type: ReplayRequest.VERSION);
                nextSegment = GetNextSegment();
                text = Encoding.UTF8.GetString(nextSegment.Data);
                break;
            case "getGameMetaData":
                _sectionBuffer = new GameMetaDataSection().Copy(_sectionBuffer, type: ReplayRequest.GAME_META_DATA);
                nextSegment = GetNextSegment();
                text = Encoding.UTF8.GetString(nextSegment.Data);
                break;
            case "getLastChunkInfo":    // /RestServicePath + /consumer/getGameDataChunk + /platformID + /gameID + /minTimeAvailable + /AccessToken
                _sectionBuffer = new LastChunkInfoSection()
                {
                    MinTimeAvailable = int.Parse(api[7])
                }.Copy(_sectionBuffer, type: ReplayRequest.LAST_CHUNK_INFO);
                nextSegment = GetNextSegment();
                text = Encoding.UTF8.GetString(nextSegment.Data);
                break;
            case "end":                 // /RestServicePath + /consumer/end + unknown-args
                nextSegment = GetNextSegment();
                text = Encoding.UTF8.GetString(nextSegment.Data);
                throw new NotImplementedException();
        }

        if (!string.IsNullOrEmpty(text))
        {
            (_sectionBuffer as IJsonSection)?.SetValues(text);
            switch (replayRequest)
            {
                case "version":
                    break;
                case "getGameMetaData":
                    _lrf.GameMetaData = (_sectionBuffer as GameMetaDataSection).GameMetaData;
                    Console.WriteLine($"<GAME METADATA>");
                    Console.WriteLine($"EncryptionKey: {_lrf.GameMetaData.EncryptionKey}");
                    Console.WriteLine($"DecodedEncryptionKey: {_lrf.GameMetaData.DecodedEncryptionKey}");
                    Console.WriteLine($"</GAME METADATA>");
                    break;
                case "end":                 // /RestServicePath + /consumer/end + unknown-args
                    throw new NotImplementedException();
            }
            Sections.Add(_sectionBuffer);
            return;
        }
        
        // DATA GET
        byte[] data = null;
        switch (replayRequest)
        {
            
            case "getKeyFrame":         // /RestServicePath + /consumer/getKeyFrame + /platformID + /gameID + /chunkID + /AccessToken
                _sectionBuffer = new KeyFrameSection()
                {
                    ID = int.Parse(api[6])
                }.Copy(_sectionBuffer, type: ReplayRequest.KEY_FRAME);
                data = GetNextSegment().Data;
                break;
            case "getGameDataChunk":    // /RestServicePath + /consumer/getGameDataChunk + /platformID +/gameID +/chunkID + /AccessToken
                var id = int.Parse(api[7]);
                _sectionBuffer = new GameDataChunkSection
                {
                    ID = id,
                    GameId = long.Parse(api[6]),
                    Chunk = new Chunk()
                    {
                        ID = id
                    }
                }.Copy(_sectionBuffer, type: ReplayRequest.GAME_DATA_CHUNK);
                data = GetNextSegment().Data;
                break;
        }

        if (data is not null)
        {
            var decompressed = _blowfish.DecompressToMemoryStream(data);
            using var reader = new BinaryReader(decompressed) ;
            var pkts = new List<ENetPacket>();
            while (reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var packet = ENetPacket.Create(reader);
                pkts.Add(packet);
            }
        
            switch (_sectionBuffer)
            {
                case GameDataChunkSection gameDataSection:
                    gameDataSection.Chunk.Packets.AddRange(pkts);
                    break;
                case KeyFrameSection keyFrameSection:
                    keyFrameSection.Packets.AddRange(pkts);
                    break;
            }
            Sections.Add(_sectionBuffer);
        }
        
        
    }

    protected override void Post(string request)
    {
        Console.WriteLine(request);
    }

    protected override void Head(byte[] data, float time) => Console.WriteLine("HEAD");

    protected override void Options(byte[] data, float time) => Console.WriteLine("HEAD");
    
    //<•······················•<>•······················•>

    protected override void OnGetBinary(byte[] data) { }
    
    protected override void OnGetText(byte[] data) { }
    
    protected override void OnDone(byte[] data, float time, DataSegment segment) { }

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
        Sections.Add(_sectionBuffer);
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
        Sections.Add(_sectionBuffer);
        Buffer.Clear();
        BufferExpectedLength = 0;
    }
}