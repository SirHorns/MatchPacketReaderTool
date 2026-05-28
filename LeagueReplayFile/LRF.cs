using LeagueReplayFile.Enums;
using LeagueReplayFile.Models;
using LeagueReplayFile.Models.Http;
using LeagueReplayFile.Models.Sections;
using LeagueReplayFile.Protocols.ENet;

namespace LeagueReplayFile;

public class ENetLRF : LRF
{
    
}

public class SpectatorLRF : LRF
{
    public GameMetaData GameMetaData { get; set; }
}

public class LRF
{
    public List<ENetPacket> Packets { get; set; }
    public List<Section> Sections { get;  set; }
    
    
    public Stream Stream { get; internal set; }
    public LRFType Type { get; internal set; }
    public BasicHeader BasicHeader { get; internal set; }
    public MetaData MetaData { get; internal set; }
}