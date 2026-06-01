using LeagueReplayFile.Enums;
using LeagueReplayFile.Models;

namespace LeagueReplayFile.LRFs;

public class LRF
{
    public LRFType Type { get; internal set; }
    public Version ReplayVersion { get; internal set; }
    public Version ClientVersion { get; internal set; }
    public BasicHeader BasicHeader { get; internal set; }
    public MetaData MetaData { get; internal set; }
}