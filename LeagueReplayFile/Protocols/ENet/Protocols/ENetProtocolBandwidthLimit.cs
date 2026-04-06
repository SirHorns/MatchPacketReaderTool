namespace LeagueReplayFile.Protocols.ENet.Protocols;

public class ENetProtocolBandwidthLimit : ENetProtocolBase
{
    public uint IncomingBandwidth { get; set; }
    public uint OutgoingBandwidth { get; set; }

    public ENetProtocolBandwidthLimit(ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader protocolCommandHeader, BinaryReader reader)
    {
        IncomingBandwidth = reader.ReadUInt32(true);
        OutgoingBandwidth = reader.ReadUInt32(true);
    }
}