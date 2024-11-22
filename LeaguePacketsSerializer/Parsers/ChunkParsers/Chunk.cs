using System.Collections.Generic;

namespace LeaguePacketsSerializer.Parsers.ChunkParsers;

public class Chunk
{
    public string Http { get; internal set; }
    public string Json { get; internal set; }
    public byte[] Data { get; internal set; }
    public List<ENetPacket> ENetPackets { get; } = new List<ENetPacket>();
}