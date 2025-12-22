using LeagueReplayFile.Enums;

namespace LeagueReplayFile.Structs.Sections;

public class Section
{
    public RequestTypes Type { get; internal set; }

    public string Http { get; internal set; }
        
    public byte[] Data { get; internal set; }
    public float Time { get; internal set; }
        

    public Section Copy(Section section, RequestTypes? type = null, string? http = null, byte[]? data = null, float? time = null)
    {
        Type = type ?? section.Type;
        Http = http ?? section.Http;
        Data = data ?? section.Data;
        Time = time ?? section.Time;
        return this;
    }
}