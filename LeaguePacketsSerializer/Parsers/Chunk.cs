using System.Collections.Generic;
using LeaguePacketsSerializer.Parsers.ChunkParsers;

namespace LeaguePacketsSerializer.Parsers;

public class Chunk
{
    public int ID { get; internal set; }
    public List<ENetPacket> ENetPackets { get; } = new();
}