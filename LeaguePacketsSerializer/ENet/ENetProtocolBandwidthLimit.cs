using System.IO;
using LeaguePacketsSerializer.Parsers;

namespace LeaguePacketsSerializer.ENet;

public class ENetProtocolBandwidthLimit : ENetProtocol
{
    public uint IncomingBandwidth { get; set; }
    public uint OutgoingBandwidth { get; set; }

    public ENetProtocolBandwidthLimit(ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader protocolCommandHeader, BinaryReader reader)
    {
        IncomingBandwidth = reader.ReadUInt32(true);
        OutgoingBandwidth = reader.ReadUInt32(true);
    }
}