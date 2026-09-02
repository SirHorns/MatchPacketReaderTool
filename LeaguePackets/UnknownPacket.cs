using System.Text;

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

public class Unknown1 : UnknownPacket
{
    public byte[] Header1 = new byte[4];
    public byte[] Header2 = new byte[4];
    public byte[] Header3 = new byte[4];
    public string Text = "";
    

    protected override void ReadHeader(ByteReader reader)
    {
        reader.ReadByte(); //0 byte buffer at the statrt
        Header1 = reader.ReadBytes(4);
        Header2 = reader.ReadBytes(4);
        Header3 = reader.ReadBytes(4);
    }

    protected override void ReadBody(ByteReader reader)
    {
        Text = reader.ReadSizedStringLast();
    }

    protected override void WriteHeader(ByteWriter writer)
    {
        writer.WriteByte(0);
        writer.WriteBytes(Header1);
        writer.WriteBytes(Header2);
        writer.WriteBytes(Header3);
    }

    protected override void WriteBody(ByteWriter writer)
    {
        writer.WriteSizedStringLast(Text);
    }

    /// <summary>
    /// Unknown1
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public new static UnknownPacket Create(byte[] data)
    {
        var packet = new Unknown1();
        packet.Read(data);
        return packet;
    }
}