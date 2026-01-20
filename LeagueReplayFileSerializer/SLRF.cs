using LeagueReplayFile.Enums;
using LeagueReplayFile.Models;
using LeagueReplayFile.Models.Sections;

namespace LeagueReplayFileSerializer;

public class SLRF
{
    public LRFTypes Type { get; internal set; }
    public BasicHeader BasicHeader { get; internal set; }
    public ReplayMetaData MetaData { get; internal set; }
    public List<SerializedPacket> Packets;
    public List<Section> Sections { get;  set; }

    public SLRF()
    {
        Packets = [];
    }
}