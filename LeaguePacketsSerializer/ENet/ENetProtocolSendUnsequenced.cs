using System.IO;
using LeaguePacketsSerializer.Parsers;

namespace LeaguePacketsSerializer.ENet;

public class ENetProtocolSendUnsequenced : ENetProtocol
{
    public ushort UnsequencedGroup { get; set; }
    public byte[] Data { get; set; }

    public ENetProtocolSendUnsequenced(ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader protocolCommandHeader, BinaryReader reader)
    {
        UnsequencedGroup = reader.ReadUInt16(true);
        ushort dataLength = reader.ReadUInt16(true);
        Data = reader.ReadExactBytes(dataLength);
    }
}