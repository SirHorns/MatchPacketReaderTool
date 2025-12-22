using Newtonsoft.Json;

namespace LeagueReplayFile.Structs;

public class DataOffset
{
    [JsonProperty("offset")]
    public int Offset { get; set; }

    [JsonProperty("size")]
    public int Size { get; set; }
}