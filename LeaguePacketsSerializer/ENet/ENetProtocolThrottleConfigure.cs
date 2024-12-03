using System.IO;
using LeaguePacketsSerializer.Parsers;

namespace LeaguePacketsSerializer.ENet;

public class ENetProtocolThrottleConfigure : ENetProtocol
{
    public uint PacketThrottleInterval { get; set; }
    public uint PacketThrottleAcceleration { get; set; }
    public uint PacketThrottleDeceleration { get; set; }

    public ENetProtocolThrottleConfigure(ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader protocolCommandHeader, BinaryReader reader)
    {
        PacketThrottleInterval = reader.ReadUInt32(true);
        PacketThrottleAcceleration = reader.ReadUInt32(true);
        PacketThrottleDeceleration = reader.ReadUInt32(true);
    }
}