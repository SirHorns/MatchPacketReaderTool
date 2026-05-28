using LeagueReplayFile.Models.Http;
using Newtonsoft.Json.Linq;

namespace LeagueReplayFile.Models.Sections;



public class GameMetaDataSection : Section, IJsonSection
{
    
    public string Json { get;  set; }
    public GameMetaData ReplayMetaData { get;  set; } = new GameMetaData();
    

    public void SetValues(string json)
    {
        Json = json;
        
        var job = JObject.Parse(json);
        ReplayMetaData = job.ToObject<GameMetaData>() ?? new GameMetaData();
        
        /*GameId = job.Value<int>("gameId");
        GameKey = job["gameKey"].ToObject<GameKey>() ?? new GameKey(-1, "");
        GameServerAddress = job.Value<string>("gameServerAddress") ?? "";
        Port = job.Value<int>("port");
        EncryptionKey = job.Value<string>("encryptionKey") ?? "";
        ChunkTimeInterval = job.Value<int>("chunkTimeInterval");
        StartTime = job.Value<string>("startTime") ?? "";
        GameEnded = job.Value<bool>("gameEnded");
        LastChunkId = job.Value<int>("lastChunkId");
        LastKeyFrameId = job.Value<int>("lastKeyFrameId");
        LastAvailableChunkId = job.Value<int>("lastAvailableChunkId");
        EndStartupChunkId = job.Value<int>("endStartupChunkId");
        DelayTime = job.Value<int>("delayTime");
        PendingAvailableChunkInfo = job["pendingAvailableChunkInfo"]?.ToObject<AvailableChunkInfo[]>() ?? [];
        PendingAvailableKeyFrameInfo = job["pendingAvailableKeyFrameInfo"]?.ToObject<AvailableKeyFrameInfo[]>() ?? [];
        KeyFrameTimeInterval = job.Value<int>("keyFrameTimeInterval");
        DecodedEncryptionKey = job.Value<string>("decodedEncryptionKey") ?? "";
        StartGameChunkId = job.Value<int>("startGameChunkId");
        GameLength = job.Value<int>("gameLength");
        ClientAddedLag = job.Value<int>("clientAddedLag");
        ClientBackFetchingEnabled = job.Value<bool>("clientBackFetchingEnabled");
        ClientBackFetchingFreq = job.Value<int>("clientBackFetchingFreq");
        InterestScore = job.Value<int>("interestScore");
        FeaturedGame = job.Value<bool>("featuredGame");
        CreateTime = job.Value<string>("createTime") ?? "";
        EndGameChunkId = job.Value<int>("endGameChunkId");
        EndGameKeyFrameId = job.Value<int>("endGameKeyFrameId");*/
    }
}