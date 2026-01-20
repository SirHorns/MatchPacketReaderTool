using LeagueReplayFile.Models;
using LeagueReplayFile.Protocols.ENet;

namespace LeagueReplayFile.ChunkParsers;

public interface IChunkParser
{
    List<DataSegment> Segments { get; }
    List<ENetPacket> Packets { get; }
    void Read(byte[] data);
}
