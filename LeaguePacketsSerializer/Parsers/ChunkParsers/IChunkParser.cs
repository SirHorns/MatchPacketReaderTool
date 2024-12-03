using System.Collections.Generic;
using LeaguePacketsSerializer.ENet;

namespace LeaguePacketsSerializer.Parsers.ChunkParsers;

public interface IChunkParser
{
    List<DataSegment> DataSegments { get; }
    List<ENetPacket> Packets { get; }
    void Read(byte[] data);
}
