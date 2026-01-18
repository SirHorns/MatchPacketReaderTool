using LeagueReplayFile.Enums;
using LeagueReplayFile.Protocols.ENet;
using LeagueReplayFile.Structs;
using LeagueReplayFile.Structs.Sections;

namespace LeagueReplayFile;

public class LRF
{
    public List<ENetPacket> Packets { get; set; }
    public List<Section> Sections { get;  set; }
    
    
    public Stream Stream { get; internal set; }
    public LRFTypes Type { get; internal set; }
    public BasicHeader BasicHeader { get; internal set; }
    public MetaData MetaData { get; internal set; }
}