using LeagueReplayFile.Enums;

namespace LeagueReplayFile.Models.Sections;

public class Section
{
    public RequestType Type { get; set; }

    public string Http { get; set; }
        
    public byte[] Data { get; set; }
    public float Time { get; set; }
    public DataSegment Segment { get; set; }

    

    public Section Copy(Section section, RequestType? type = null, string? http = null, byte[]? data = null, float? time = null)
    {
        Type = type ?? section.Type;
        Http = http ?? section.Http;
        Data = data ?? section.Data;
        Time = time ?? section.Time;
        return this;
    }
}