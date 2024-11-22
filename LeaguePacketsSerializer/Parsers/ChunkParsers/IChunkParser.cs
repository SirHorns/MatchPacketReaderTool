using System.Collections.Generic;

namespace LeaguePacketsSerializer.Parsers.ChunkParsers;

public interface IChunkParser
{
    List<Chunk> Chunks { get; }
    List<ENetPacket> Packets { get; }
    void Parse(byte[] data);
}
