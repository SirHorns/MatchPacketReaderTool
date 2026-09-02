using LeagueReplayFile.Enums;
using LeagueReplayFile.Models;
using LeagueReplayFile.Models.Sections;
using LeagueReplayFileSerializer.Data;

namespace LeagueReplayFileSerializer;

public class SLRF
{
    public LRFType Type { get; internal set; }
    public BasicHeader BasicHeader { get; internal set; }
    public MetaData MetaData { get; internal set; }
    public List<SerializedPacket> Packets;
    public List<SerializedSection> Sections { get;  set; }

    public SLRF()
    {
        Packets = [];
    }
}