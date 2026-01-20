using LeagueReplayFile.Protocols.ENet;

namespace LeagueReplayFile.Models.Sections;

public class KeyFrameSection : Section
{
    public int ID { get; internal set; }
    public List<ENetPacket> Packets { get; set; } = [];
}