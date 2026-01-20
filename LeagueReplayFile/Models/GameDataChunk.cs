using LeagueReplayFile.Protocols.ENet;

namespace LeagueReplayFile.Models;

public class GameDataChunk
{
    public int ID { get; set; }
    public List<ENetPacket> Packets { get; set; } = [];
}