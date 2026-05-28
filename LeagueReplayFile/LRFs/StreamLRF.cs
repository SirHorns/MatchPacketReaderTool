using LeagueReplayFile.Protocols.ENet;

namespace LeagueReplayFile.LRFs;

public class StreamLRF : LRF
{
    public List<ENetPacket> Packets { get; set; }
}