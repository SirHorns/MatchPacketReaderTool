using System.Collections.Generic;

namespace LeaguePacketsSerializer.Parsers.ChunkParsers;

public interface IChunkParser
{
    List<ENetPacket> Packets { get; }
    void Read(byte[] data);
}
