using LeaguePacketsSerializer.Enums;
using LeaguePacketsSerializer.Parsers.ChunkParsers;

namespace LeaguePacketsSerializer.Parsers;

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

public class VersionSection : Section
{
    public string Text { get; internal set; }
}

public class MetaDataSection : Section
{
    public int MatchId { get; internal set; }
    public string Json { get; internal set; }
}
    
public class LastChunkInfoSection : Section
{
    public int Unknown { get; internal set; }
    public string Json { get; internal set; }
}

public class KeyFrameSection : Section
{
    public int ID { get; internal set; }
}

public class GameDataSection : Section
{
    public int ID { get; internal set; }
    public Chunk Chunk { get; internal set; } = new Chunk();
}
