using Newtonsoft.Json.Linq;

namespace LeagueReplayFile.Models.Sections;

public class LastChunkInfoSection : Section, IJsonSection
{
    public int MinTimeAvailable { get; set; }
    public string Json { get; set; } = "";
    public LastChunkInfo LastChunkInfo { get; set; } = new LastChunkInfo(); 
    
    
    public void SetValues(string json)
    {
        Json = json;
        var job = JObject.Parse(json);
        LastChunkInfo = job.ToObject<LastChunkInfo>();
        
        /*ChunkId = job.Value<int>("chunkId");
        AvailableSince = job.Value<float>("availableSince");
        NextAvailableChunk = job.Value<int>("nextAvailableChunk");
        KeyFrameId = job.Value<int>("keyFrameId");
        NextChunkId = job.Value<int>("nextChunkId");
        EndStartupChunkId = job.Value<int>("endStartupChunkId");
        StartGameChunkId = job.Value<int>("startGameChunkId");
        EndGameChunkId = job.Value<int>("endGameChunkId");
        Duration = job.Value<float>("duration");*/
    }
}