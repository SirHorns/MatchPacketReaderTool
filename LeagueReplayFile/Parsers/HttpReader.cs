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
using Newtonsoft.Json;

namespace LeagueReplayFile.Parsers;

/// <summary>
/// Parses replays sent over HTTP
/// </summary>
public class HttpReader : HttpProtocol
{
    private readonly BlowFish _blowfish;
    
    private RequestType _currentRequestType;
    private HttpState _currentHttpState;
    private Section? _currentSection;
    private bool _checkForMaestroData;
    private MaestroMessage? _lastMaestroMessage;
    private HttpLRF _lrf;
    
    private List<byte> Buffer { get; }
    private long BufferExpectedLength { get; set; }
    
    public List<ENetPacket> Packets { get; }
    public List<Section> Sections { get; }
    
    public HttpReader(HttpLRF lrf)
    {
        _lrf = lrf;
        var encryptionKey = lrf.MetaData.EncryptionKey;
        var matchId = lrf.MetaData.MatchId;
        
        _currentRequestType = RequestType.NONE;
        _currentHttpState = HttpState.Done;
        _currentSection = null;
        
        var checksumKey = Encoding.ASCII.GetBytes(matchId.ToString());
        var checksumBlowfish = new BlowFish(checksumKey);
        var realKey = checksumBlowfish.Decrypt(encryptionKey);
        _blowfish = new BlowFish(realKey.Take(16).ToArray());
        
        Sections = [];
        Packets = [];
        Buffer = [];
    }

    private bool _read;
    

    public void Read(byte[] replayBytes)
    {
        byte[] streamBytes;
        var streamOffset = _lrf.MetaData.DataIndex[0].Value;
        if (replayBytes.Length < streamOffset.Size)
        {
            Console.Error.WriteLine($"[WARNING]: Stream data size ({streamOffset.Size}) is larger than " +
                                    $"recorded data size ({replayBytes.Length})! Defaulting to read all bytes.");
            streamBytes = new byte[replayBytes.Length];
            Array.Copy(replayBytes, streamBytes, replayBytes.Length);
        }
        else
        {
            streamBytes = new byte[streamOffset.Size];
            Array.Copy(replayBytes, streamOffset.Offset, streamBytes, 0, streamOffset.Size);
        }
        using var reader = new BinaryReader(new MemoryStream(streamBytes));

        if (_lrf.ReplayVersion > LRFReader.SpectatorVersion)
        {
            ReadSpectatorIdStream(reader);
        }
        else
        {
            ReadStream(reader);
        }
    }

    private void ReadSpectatorIdStream(BinaryReader reader)
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
        List<DataSegment> segments = [];
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var segment = DataSegment.Read(reader);
            segments.Add(segment);
            ParseSegment(segment);
        }
    }

    private void ParseSegment(DataSegment segment)
    {
        var data = segment.Data;
        var time = segment.Time;
        switch (_currentHttpState)
        {
            case HttpState.GetBinary:
                OnGetBinary(data);
                SetHttpState(HttpState.Done);
                break;
            case HttpState.GetText:
                OnGetText(data);
                SetHttpState(HttpState.Done);
                break;
            case HttpState.Done:
                OnDone(data, time, segment);
                break;
            case HttpState.ContinueBinary:
                OnContinueBinary(data);
                break;
            case HttpState.ContinueText:
                OnContinueText(data);
                break;
            default:
                Console.WriteLine($"Skipped Segment: {_currentHttpState}");
                break;
        }
    }

    private void SetHttpState(HttpState state) => _currentHttpState = state;
    
    private void SetCurrentRequest(RequestType type) => _currentRequestType = type;

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
    
    protected override void Get(string request)
    {
        // /observer-mode/rest/consumer/<api-call>/
        if (request.Equals("\r\n"))
        {
            SetHttpState(HttpState.Done);
            Sections.Add(_currentSection);
            return;
        }

        var api = request.Split("/");
        var http = _currentSection.Http.Split("/");
        switch (api[4])
        {
            case "version":
                SetCurrentRequest(RequestType.VERSION);
                SetHttpState(HttpState.GetText);
                _currentSection = new VersionSection().Copy(_currentSection, type: RequestType.VERSION);
                break;
            case "getGameMetaData":
                SetCurrentRequest(RequestType.GAME_META_DATA);
                SetHttpState(HttpState.GetText);
                _currentSection = new GameMetaDataSection().Copy(_currentSection, type: RequestType.GAME_META_DATA);
                break;
            case "getLastChunkInfo":    // /RestServicePath + /consumer/getGameDataChunk + /platformID + /gameID + /minTimeAvailable + /AccessToken
                SetCurrentRequest(RequestType.LAST_CHUNK_INFO);
                SetHttpState(HttpState.GetText);
                _currentSection = new LastChunkInfoSection()
                {
                    MinTimeAvailable = int.Parse(http[^2])
                }.Copy(_currentSection, type: RequestType.LAST_CHUNK_INFO);
                break;
            case "getKeyFrame":         // /RestServicePath + /consumer/getKeyFrame + /platformID + /gameID + /chunkID + /AccessToken
                SetCurrentRequest(RequestType.KEY_FRAME);
                SetHttpState(HttpState.GetBinary);
                _currentSection = new KeyFrameSection()
                {
                    ID = int.Parse(http[^2])
                }.Copy(_currentSection, type: RequestType.KEY_FRAME);
                break;
            case "getGameDataChunk":    // /RestServicePath + /consumer/getGameDataChunk + /platformID +/gameID +/chunkID + /AccessToken
                SetCurrentRequest(RequestType.GAME_DATA_CHUNK);
                SetHttpState(HttpState.GetBinary);
                var id = int.Parse(http[^2]);
                var gameId = long.Parse(http[^3]);
                _currentSection = new GameDataChunkSection
                {
                    ID = id,
                    GameId = gameId,
                    Chunk = new Chunk()
                    {
                        ID = id
                    }
                }.Copy(_currentSection, type: RequestType.GAME_DATA_CHUNK);
                break;
            case "end":                 // /RestServicePath + /consumer/end + unknown-args
                throw new NotImplementedException("end (OfGameStats) is not implemented yet");
            default:
                _currentSection.Type = RequestType.NONE;
                Console.WriteLine(request);
                break;
        }
    }

    protected override void Post(string request)
    {
        Console.WriteLine(request);
        SetHttpState(HttpState.GetText);
    }

    protected override void Head(byte[] data, float time) => Console.WriteLine("HEAD");

    protected override void Options(byte[] data, float time) => Console.WriteLine("HEAD");
    
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
        
        switch (_currentSection)
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
        
        Sections.Add(_currentSection);
    }
    
    protected override void OnGetText(byte[] data)
    {
        switch (_currentRequestType)
        {
            case RequestType.END_OF_GAME_STATS:
            case RequestType.KEY_FRAME:
            case RequestType.GAME_DATA_CHUNK:
            case RequestType.NONE:
                Console.WriteLine($"Attempted to get text from non-text section!: {_currentRequestType}");
                return;
        }

        var json = Encoding.UTF8.GetString(data);
        (_currentSection as IJsonSection)?.SetValues(json);
        switch (_currentRequestType)
        {
            case RequestType.VERSION:
                _lrf.Version = json;
                break;
            case RequestType.GAME_META_DATA:
                _lrf.GameMetaData = (_currentSection as GameMetaDataSection).GameMetaData;
                Console.WriteLine($"<GAME METADATA>");
                Console.WriteLine($"EncryptionKey: {_lrf.GameMetaData.EncryptionKey}");
                Console.WriteLine($"DecodedEncryptionKey: {_lrf.GameMetaData.DecodedEncryptionKey}");
                Console.WriteLine($"</GAME METADATA>");
                break;
            case RequestType.LAST_CHUNK_INFO:
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
        _currentSection = new Section()
        {
            Segment = segment,
        };

        if (data.Length == 16) // Maestro message packets ate four sets of 8 bytes longs (16 bytes)
        {
            var message = MaestroMessage.Read(data);
            _lastMaestroMessage = message;
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
                    _checkForMaestroData = true;
                    break;
                default:
                    break;
            }
            return;
        }

        var text = Encoding.UTF8.GetString(data);
        if (_checkForMaestroData)
        {
            _lastMaestroMessage.ExtraBytes = data;
            _checkForMaestroData = false;
            return;
        }
        
        
        var req = text.Split(' ');
        var httpReq = req[0];
        
        var replayReq = req[1];

        _currentSection.Http = replayReq;
        _currentSection.Data = data;
        _currentSection.Time = time;
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
        
        Sections.Add(_currentSection);
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
        Sections.Add(_currentSection);
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
        Sections.Add(_currentSection);
        Buffer.Clear();
        BufferExpectedLength = 0;
    }
}