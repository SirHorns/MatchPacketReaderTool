namespace LeagueReplayFile.Protocols.ENet.Protocols;

public class ENetProtocolDisconnect : ENetProtocolBase
{
    public uint Data { get; set; }

    public ENetProtocolDisconnect(ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader protocolCommandHeader, BinaryReader reader)
    {
        Data = reader.ReadUInt32(true);
    }
}