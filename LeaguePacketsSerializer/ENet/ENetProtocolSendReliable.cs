using System.IO;
using LeaguePacketsSerializer.Parsers;

namespace LeaguePacketsSerializer.ENet;

public class ENetProtocolSendReliable : ENetProtocol
{
    public byte[] Data { get; set; }

    public ENetProtocolSendReliable(ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader protocolCommandHeader, BinaryReader reader)
    {
        ushort dataLength = reader.ReadUInt16(true);
        Data = reader.ReadExactBytes(dataLength);
    }
}