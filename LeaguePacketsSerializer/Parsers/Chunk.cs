using System.Collections.Generic;
using LeaguePacketsSerializer.ENet;
using LeaguePacketsSerializer.Packets;
using LeaguePacketsSerializer.Parsers.ChunkParsers;

namespace LeaguePacketsSerializer.Parsers;

public class Chunk
{
    public int ID { get; set; }
    public List<ENetPacket> ENetPackets { get; set; } = new();
    public List<SerializedPacket> SerializedPackets { get; set; } = new();
    public List<BadPacket> HardBadPackets { get; set; } = new();
    public List<BadPacket> SoftBadPackets { get; set; } = new();
}