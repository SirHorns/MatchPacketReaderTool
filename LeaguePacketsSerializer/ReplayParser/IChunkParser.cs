using System.Collections.Generic;

namespace LeaguePacketsSerializer.ReplayParser;

public interface IChunkParser
{
    List<ENetPacket> Packets { get; }
    void Read(byte[] data, float time);
}
