using Newtonsoft.Json;

namespace LeaguePacketsSerializer.ReplayParser;

public class DataOffset
{
    [JsonProperty("offset")]
    public int Offset { get; set; }

    [JsonProperty("size")]
    public int Size { get; set; }
}