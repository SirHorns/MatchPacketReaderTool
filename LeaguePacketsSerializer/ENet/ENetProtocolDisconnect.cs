using System.IO;
using LeaguePacketsSerializer.Parsers;

namespace LeaguePacketsSerializer.ENet;

public class ENetProtocolDisconnect : ENetProtocol
{
    public uint Data { get; set; }

    public ENetProtocolDisconnect(ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader protocolCommandHeader, BinaryReader reader)
    {
        Data = reader.ReadUInt32(true);
    }
}