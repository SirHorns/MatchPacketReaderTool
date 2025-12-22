using LeagueReplayFile.Protocols.ENet;
using LeagueReplayFile.Structs;

namespace LeagueReplayFile.ChunkParsers;

public interface IChunkParser
{
    List<DataSegment> Segments { get; }
    List<ENetPacket> Packets { get; }
    void Read(byte[] data);
}
