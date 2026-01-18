using LeagueReplayFile.Protocols.ENet;

namespace LeagueReplayFile.Structs;

public class Chunk
{
    public int ID { get; set; }
    public List<ENetPacket> Packets { get; set; } = [];
}