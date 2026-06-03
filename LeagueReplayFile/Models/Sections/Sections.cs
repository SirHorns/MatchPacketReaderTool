using LeagueReplayFile.Enums;

namespace LeagueReplayFile.Models.Sections;

public class Section
{
    public ReplayRequest Request { get; set; }
    public string Http { get; set; }
    public string Api { get; set; }
    public byte[] Data { get; set; }
    public float Time { get; set; }
    public DataSegment Segment { get; set; }

    

    public Section Copy(Section section, ReplayRequest? type = null, string? http = null, byte[]? data = null, float? time = null)
    {
        Request = type ?? section.Request;
        Http = http ?? section.Http;
        Data = data ?? section.Data;
        Time = time ?? section.Time;
        Segment = section.Segment;
        return this;
    }
}