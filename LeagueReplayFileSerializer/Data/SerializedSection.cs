using LeagueReplayFile.Enums;
using LeagueReplayFile.Models;

namespace LeagueReplayFileSerializer.Data;

public class SerializedSection
{
    public ReplayRequest Request { get; set; }
    public string Http { get; set; }
    public string Api { get; set; }
    public byte[] Data { get; set; }
    public float Time { get; set; }
    public DataSegment Segment { get; set; }
}