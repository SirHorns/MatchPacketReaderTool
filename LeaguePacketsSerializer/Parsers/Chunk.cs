using System.Collections.Generic;
using LeaguePacketsSerializer.ENet;
using LeaguePacketsSerializer.Packets;
using LeaguePacketsSerializer.Parsers.ChunkParsers;

namespace LeaguePacketsSerializer.Parsers;

public class Chunk
{
    public int ID { get; internal set; }
    public List<ENetPacket> ENetPackets { get; } = new();
    public List<SerializedPacket> SerializedPackets { get; } = new();
    public List<BadPacket> HardBadPackets { get; } = new();
    public List<BadPacket> SoftBadPackets { get; } = new();
}