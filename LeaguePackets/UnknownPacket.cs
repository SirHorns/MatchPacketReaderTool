namespace LeaguePackets;

/// <summary>
/// Packet that doest not follow known patterns
/// </summary>
public class UnknownPacket: BasePacket
{
    protected override void ReadHeader(ByteReader reader) { }

    protected override void ReadBody(ByteReader reader) { }

    protected override void WriteHeader(ByteWriter writer) { }

    protected override void WriteBody(ByteWriter writer) { }

    public static UnknownPacket Create(byte[] data)
    {
        var packet = new UnknownPacket();
        packet.Read(data);
        return packet;
    }
}