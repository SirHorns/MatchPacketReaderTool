using Newtonsoft.Json;

namespace LeaguePacketsSerializer.Parsers;

public class DataOffset
{
    [JsonProperty("offset")]
    public int Offset { get; set; }

    [JsonProperty("size")]
    public int Size { get; set; }
}