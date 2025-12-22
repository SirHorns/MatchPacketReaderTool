using LeagueReplayFile.Protocols.ENet;

namespace LeagueReplayFile.Structs.Sections;

public class KeyFrameSection : Section
{
    public int ID { get; internal set; }
    public List<ENetPacket> ENetPackets { get; set; } = [];
}