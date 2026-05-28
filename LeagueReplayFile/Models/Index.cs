using Newtonsoft.Json;

namespace LeagueReplayFile.Models;

public class Index
{
    [JsonProperty("Value")]
    public IndexValue Value { get; set; }
    public string Key { get; set; }

    
    public class IndexValue
    {
        public long Size { get; set; }
        public long Offset { get; set; }
    }
}