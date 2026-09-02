using LeagueReplayFile.Models.Sections;

namespace LeagueReplayFileSerializer.Data;

public class SerializedKeyFrameSection : SerializedSection
{
    public int ID { get; internal set; }
    public List<SerializedPacket> Packets { get; set; } = [];
}