using LeagueReplayFile.Models;
using Microsoft.AspNetCore.Mvc;

namespace SpectatorAPI.Controllers;

public delegate Task<string> GetStringHandle();
public delegate Task<GameMetaData> GetMetaDataHandle();
public delegate Task<LastChunkInfo> GetLstChunkInfoHandle(string unknown);
public delegate Task<string> GetGameDataChunkHandle(int chunkID);
public delegate Task<KeyFrame> GetKeyFrameHandle(int frameID);

[ApiController]
[Route("observer-mode/rest/consumer/")]
public class SpectatorReplayController : ControllerBase
{
    private readonly ILogger<SpectatorReplayController> _logger;
    
    public static event GetStringHandle OnFeatured;
    public static event GetStringHandle OnVersion;
    public static event GetMetaDataHandle OnGetMetadata;
    public static event GetLstChunkInfoHandle OnLasChunkInfo;
    public static event GetGameDataChunkHandle OnGameDataChunk;
    public static event GetKeyFrameHandle OnKeyFrame;

    public SpectatorReplayController(ILogger<SpectatorReplayController> logger)
    {
        _logger = logger;
    }

    [HttpGet("featured", Name = "Featured")]
    public async Task<string> Featured()
    {
        string featured = await OnFeatured.Invoke();
        _logger.Log(LogLevel.Information, $"[featured] {featured}");
        return featured;
    }

    [HttpGet("version", Name = "Version")]
    public async Task<string> GetVersion()
    {
        var version = await OnVersion.Invoke();
        _logger.Log(LogLevel.Information, $"[version] {version}");
        return version;
    }
    
    [HttpGet("getLastChunkInfo/{platformId}/{gameId}/null", Name = "EndOfGameStats")]
    public async Task<string> GetEndOfGameStats()
    {
        return "";
    }

    [HttpGet("getGameMetaData/{platformId}/{matchId}/{unknown1}/token", Name = "GameMetaData")]
    public async Task<GameMetaData> GetGameMetaData(string platformId, string matchId, string unknown1)
    {
        _logger.Log(LogLevel.Information,
            $"[getGameMetaData] Platform: {platformId} MatchId: {matchId} Unknown1: {unknown1}");
        var res = await OnGetMetadata.Invoke();
        return res;
        /*return new GameMetaData()
        {
            GameKey = new GameKey()
            {
                GameId = 0,
                PlatformId = "NA1"
            },
            GameServerAddress = "",
            Port = 0,
            EncryptionKey = "",
            ChunkTimeInterval = 30000,
            StartTime = "Dec 31, 1969 4:00:00 PM",
            GameEnded = false,
            LastChunkId = 3,
            LastKeyFrameId = -1,
            EndStartupChunkId = 2,
            DelayTime = 150000,
            PendingAvailableChunkInfo =
            [
                new PendingAvailableChunkInfo()
                {
                    Id = 1,
                    Duration = 30119,
                    ReceivedTime = "Oct 29, 2014 8:31:01 PM"
                },
                new PendingAvailableChunkInfo()
                {
                    Id = 2,
                    Duration = 21534,
                    ReceivedTime = "Oct 29, 2014 8:31:23 PM"
                },
                new PendingAvailableChunkInfo()
                {
                    Id = 3,
                    Duration = 0,
                    ReceivedTime = "Oct 29, 2014 8:31:32 PM"
                }
            ],
            PendingAvailableKeyFrameInfo = [],
            KeyFrameTimeInterval = 60000,
            DecodedEncryptionKey = "",
            StartGameChunkId = 0,
            GameLength = 0,
            ClientAddedLag = 30000,
            ClientBackFetchingEnabled = false,
            ClientBackFetchingFreq = 1000,
            InterestScore = 1462,
            FeaturedGame = false,
            CreateTime = "Oct 29, 2014 8:30:31 PM",
            EndGameChunkId = -1,
            EndGameKeyFrameId = -1
        };*/
    }

    [HttpGet("getLastChunkInfo/{platformId}/{matchId}/{unknown1}/token", Name = "LastChunkInfo")]
    public async Task<LastChunkInfo> GetLastChunkInfo(string platformId, string matchId, string unknown1)
    {
        _logger.Log(LogLevel.Information,$"[getLastChunkInfo] Platform: {platformId} MatchId: {matchId} Unknown1: {unknown1}");
        var res = await OnLasChunkInfo.Invoke(unknown1);
        return new LastChunkInfo();
    }

    [HttpGet("getGameDataChunk/{platformId}/{matchId}/{chunkId}/token", Name = "GameDataChunk")]
    public GameDataChunk GetGameDataChunk(string platformId, string matchId, string chunkId)
    {
        _logger.Log(LogLevel.Information, $"[getGameDataChunk] Platform: {platformId} MatchId: {matchId} ChunkID: {chunkId}");
        return new GameDataChunk();
    }

    [HttpGet("getKeyFrame/{platformId}/{matchId}/{frameId}/token", Name = "KeyFrame")]
    public KeyFrame GetKeyFrame(string platformId, string matchId, string frameId)
    {
        _logger.Log(LogLevel.Information, $"[getKeyFrame] Platform: {platformId} MatchId: {matchId} FrameID: {frameId}");
        return new KeyFrame();
    }
}