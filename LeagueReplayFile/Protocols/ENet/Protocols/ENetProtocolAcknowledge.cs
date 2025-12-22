namespace LeagueReplayFile.Protocols.ENet.Protocols;

public class ENetProtocolAcknowledge : ENetProtocol
{
    public ushort ReceivedReliableSequenceNumber { get; set; }
    public ushort ReceivedSentTime { get; set; }
    public ENetProtocolAcknowledge(ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader protocolCommandHeader, BinaryReader reader)
    {
        ReceivedReliableSequenceNumber = reader.ReadUInt16(true);
        ReceivedSentTime = reader.ReadUInt16(true);
    }
}