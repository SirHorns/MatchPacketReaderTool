using LeaguePacketsSerializer.Packets;
using LeagueReplayFile.Enums;
using LeagueReplayFile.Structs;

namespace LeaguePacketsSerializer;

public class SLRF
{
    public LRFTypes Type { get; internal set; }
    public BasicHeader BasicHeader { get; internal set; }
    public MetaData MetaData { get; internal set; }
    public List<SerializedPacket> Packets;

    public SLRF()
    {
        Packets = [];
    }
}