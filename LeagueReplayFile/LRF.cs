using LeagueReplayFile.Enums;
using LeagueReplayFile.Protocols.ENet;
using LeagueReplayFile.Structs;
using LeagueReplayFile.Structs.Sections;

namespace LeagueReplayFile;

public class LRF
{
    internal List<ENetPacket> Packets { get; set; }
    internal List<Section> Sections { get;  set; }
    
    
    public Stream Stream { get; internal set; }
    public LRFTypes Type { get; internal set; }
    public BasicHeader BasicHeader { get; internal set; }
    public MetaData MetaData { get; internal set; }
    public IReadOnlyList<ENetPacket> ENetPackets => Packets.AsReadOnly();
    public IReadOnlyList<Section> ReplaySections => Sections.AsReadOnly();
}