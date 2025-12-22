namespace LeagueReplayFile.Protocols.ENet.Protocols;

public class ENetProtocolHeader
{
    public uint? CheckSum { get; set; } = null;
    public uint SessionID { get; set; }
    public ushort? PeerID { get; set; } = null;
    public ushort? TimeSent { get; set; } = null;
    public float TimeRecieved { get; set; }
    public ENetGameClientVersions ENetGameClientVersions { get; set; }

    public static readonly Dictionary<ENetGameClientVersions, int> ProtocolHeaderSizes = new Dictionary<ENetGameClientVersions, int>
    {
        [ENetGameClientVersions.Seasson12] = 8,
        [ENetGameClientVersions.Seasson34] = 4,
        [ENetGameClientVersions.Patch4] = 8,
    };

    public ENetProtocolHeader(BinaryReader reader, float timeRecieved, ENetGameClientVersions enetGameClientVersions)
    {
        switch (enetGameClientVersions)
        {
            case ENetGameClientVersions.Seasson12:
            {
                SessionID = reader.ReadUInt32(true);
                ushort peerID = reader.ReadUInt16(true);
                if((peerID & 0x7FFF) != 0x7FFF)
                {
                    PeerID = peerID;
                }
                if ((peerID & 0x8000) > 0)
                {
                    TimeSent = reader.ReadUInt16();
                }
            }
                break;
            case ENetGameClientVersions.Seasson34:
            {
                SessionID = reader.ReadByte();
                var peerId = reader.ReadByte();
                if ((peerId & 0x7F) != 0x7F)
                {
                    PeerID = peerId;
                }
                if ((peerId & 0x80) > 0)
                {
                    TimeSent = reader.ReadUInt16();
                }
            }
                break;
            case ENetGameClientVersions.Patch4:
            case ENetGameClientVersions.Patch5:
            case ENetGameClientVersions.Patch6:
            case ENetGameClientVersions.Patch7:
            case ENetGameClientVersions.Patch8:
            {
                CheckSum = reader.ReadUInt32(true);
                SessionID = reader.ReadByte();
                var peerId = reader.ReadByte();
                if ((peerId & 0x7F) != 0x7F)
                {
                    PeerID = peerId;
                }
                if ((peerId & 0x80) > 0)
                {
                    TimeSent = reader.ReadUInt16();
                }
            }
                break;
            case ENetGameClientVersions.Unknown:
            default:
                throw new NotImplementedException();
        }
        TimeRecieved = timeRecieved;
        ENetGameClientVersions = enetGameClientVersions;
    }
}