using LeagueReplayFile.Enums;
using LeagueReplayFile.Models;
using LeagueReplayFile.Models.Sections;
using LeagueReplayFile.Protocols.ENet;

namespace LeagueReplayFile;

public class LRF
{
    public List<ENetPacket> Packets { get; set; }
    public List<Section> Sections { get;  set; }
    
    
    public Stream Stream { get; internal set; }
    public LRFTypes Type { get; internal set; }
    public BasicHeader BasicHeader { get; internal set; }
    public ReplayMetaData MetaData { get; internal set; }
}