using System.IO;
using LeaguePacketsSerializer.Parsers;

namespace LeaguePacketsSerializer.ENet;

public class ENetProtocolCommandHeader
{
    public ENetCommandFlag Flags { get; set; }
    public ENetProtocolCommand Command { get; set; }
    public byte Channel { get; set; }
    public ushort ReliableSequenceNumber { get; set; }

    public const int CommandHeaderSize = 4;
    public ENetProtocolCommandHeader(BinaryReader reader)
    {
        byte command_flags = reader.ReadByte();
        Flags = (ENetCommandFlag)(byte)(command_flags & 0xF0);
        Command = (ENetProtocolCommand)(byte)(command_flags & 0x0F);
        Channel = reader.ReadByte();
        ReliableSequenceNumber = reader.ReadUInt16(true);
    }
}