namespace LeagueReplayFile.Protocols.ENet.Protocols;

public class ENetProtocolHeader
{
    public uint? CheckSum { get; set; } = null;
    public uint SessionID { get; set; }
    public ushort? PeerID { get; set; } = null;
    public ushort? TimeSent { get; set; } = null;
    public float TimeRecieved { get; set; }
    public ENetGameClientVersions ENetGameClientVersions { get; set; }

    public static Dictionary<ENetGameClientVersions, int> ProtocolHeaderSizes { get; }

    static ENetProtocolHeader()
    {
        ProtocolHeaderSizes = [];
        var values = Enum.GetValues<ENetGameClientVersions>();
        foreach (var value in values)
        {
            ProtocolHeaderSizes[value] = 8;
            switch (value)
            {
                case ENetGameClientVersions.Unknown:
                    break;
                case ENetGameClientVersions.Patch1:
                    break;
                case ENetGameClientVersions.Patch2:
                    break;
                case ENetGameClientVersions.Patch3:
                    break;
                case ENetGameClientVersions.Patch4:
                    ProtocolHeaderSizes[ENetGameClientVersions.Patch4] = 8;
                    break;
                case ENetGameClientVersions.Patch5:
                    break;
                case ENetGameClientVersions.Patch6:
                    break;
                case ENetGameClientVersions.Patch7:
                    break;
                case ENetGameClientVersions.Patch8:
                    break;
                case ENetGameClientVersions.Seasson12:
                    ProtocolHeaderSizes[ENetGameClientVersions.Seasson12] = 8;
                    break;
                case ENetGameClientVersions.Seasson34:
                    ProtocolHeaderSizes[ENetGameClientVersions.Seasson34] = 4;
                    break;
                default:
                    break;
            }
        }
    }

    public ENetProtocolHeader() { }

    public static ENetProtocolHeader Read(BinaryReader reader, float timeReceived, ENetGameClientVersions enetGameClientVersions)
    {
        uint sessionID;
        ushort? peerID = null;
        ushort? timeSent = null;
        uint? checksum = null;
        switch (enetGameClientVersions)
        {
            case ENetGameClientVersions.Seasson12:
            {
                sessionID = reader.ReadUInt32(true);
                var id = reader.ReadUInt16(true);
                if((peerID & 0x7FFF) != 0x7FFF)
                {
                    peerID = id;
                }
                if ((id & 0x8000) > 0)
                {
                    timeSent = reader.ReadUInt16();
                }
            }
                break;
            case ENetGameClientVersions.Seasson34:
            {
                sessionID = reader.ReadByte();
                var id = reader.ReadByte();
                if ((id & 0x7F) != 0x7F)
                {
                    peerID = id;
                }
                if ((id & 0x80) > 0)
                {
                    timeSent = reader.ReadUInt16();
                }
            }
                break;
            case ENetGameClientVersions.Patch4:
            case ENetGameClientVersions.Patch5:
            case ENetGameClientVersions.Patch6:
            case ENetGameClientVersions.Patch7:
            case ENetGameClientVersions.Patch8:
            {
                checksum = reader.ReadUInt32(true);
                sessionID = reader.ReadByte();
                var id = reader.ReadByte();
                if ((id & 0x7F) != 0x7F)
                {
                    peerID = id;
                }
                if ((id & 0x80) > 0)
                {
                    timeSent = reader.ReadUInt16();
                }
            }
                break;
            case ENetGameClientVersions.Unknown:
            default:
                throw new NotImplementedException();
        }
        

        var header = new ENetProtocolHeader()
        {
            SessionID = sessionID,
            CheckSum = checksum,
            PeerID = peerID,
            TimeSent = timeSent,
            TimeRecieved = timeReceived,
            ENetGameClientVersions = enetGameClientVersions
        };
        
        return header;
    }
}