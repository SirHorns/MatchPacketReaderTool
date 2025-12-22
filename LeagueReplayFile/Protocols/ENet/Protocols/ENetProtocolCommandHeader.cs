namespace LeagueReplayFile.Protocols.ENet.Protocols;

public class ENetProtocolCommandHeader
{
    public ENetCommandFlags Flags { get; set; }
    public ENetProtocolCommand Command { get; set; }
    public byte Channel { get; set; }
    public ushort ReliableSequenceNumber { get; set; }

    public const int CommandHeaderSize = 4;
    public ENetProtocolCommandHeader(BinaryReader reader)
    {
        byte command_flags = reader.ReadByte();
        Flags = (ENetCommandFlags)(byte)(command_flags & 0xF0);
        Command = (ENetProtocolCommand)(byte)(command_flags & 0x0F);
        Channel = reader.ReadByte();
        ReliableSequenceNumber = reader.ReadUInt16(true);
    }
}