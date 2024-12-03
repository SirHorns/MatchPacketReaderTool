using System.IO;
using LeaguePacketsSerializer.Parsers;

namespace LeaguePacketsSerializer.ENet;

public class ENetProtocolSendUnreliable : ENetProtocol
{
    public ushort UnreliableSequenceNumber { get; set; }
    public byte[] Data { get; set; }

    public ENetProtocolSendUnreliable(ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader protocolCommandHeader, BinaryReader reader)
    {
        UnreliableSequenceNumber = reader.ReadUInt16(true);
        ushort dataLength = reader.ReadUInt16(true);
        Data = reader.ReadExactBytes(dataLength);
    }
}