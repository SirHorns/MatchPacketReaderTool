using LeagueReplayFile.Models;
using Microsoft.AspNetCore.Mvc;

namespace SpectatorAPI.Controllers;

public delegate Task<string> GetStringHandle();
public delegate Task<EndOfGameStats> GetEndOfGameStatsHandle(string platformId, long gameId);
public delegate Task<GameMetaData> GetMetaDataHandle(string platformId, long gameId, string unknown);
public delegate Task<LastChunkInfo> GetLastChunkInfoHandle(string platformId, long gameId, string unknown);
public delegate Task<byte[]> GetGameDataChunkHandle(string platformId, long gameId, int chunkId);
public delegate Task<byte[]> GetKeyFrameHandle(string platformId, long gameId, int frameId);

[ApiController]
[Route("observer-mode/rest/consumer/")]
public class SpectatorReplayController : ControllerBase
{
    private readonly ILogger<SpectatorReplayController> _logger;
    
    public static event GetStringHandle OnFeatured;
    public static event GetEndOfGameStatsHandle OnEndOfGameStats;
    public static event GetStringHandle OnVersion;
    public static event GetMetaDataHandle OnGetMetadata;
    public static event GetLastChunkInfoHandle OnLasChunkInfo;
    public static event GetGameDataChunkHandle OnGameDataChunk;
    public static event GetKeyFrameHandle OnKeyFrame;

    public SpectatorReplayController(ILogger<SpectatorReplayController> logger)
    {
        _logger = logger;
    }

    [HttpGet("featured", Name = "Featured")]
    public async Task<string> Featured()
    {
        var result = await OnFeatured.Invoke();
        _logger.Log(LogLevel.Information, $"[featured] {result}");
        return result;
    }

    [HttpGet("version", Name = "Version")]
    public async Task<string> GetVersion()
    {
        var result = await OnVersion.Invoke();
        _logger.Log(LogLevel.Information, $"[version] {result}");
        return result;
    }
    
    [HttpGet("getEndOfGameStats/{platformId}/{gameId}/null", Name = "EndOfGameStats")]
    public async Task<EndOfGameStats> GetEndOfGameStats(string platformId, string gameId)
    {
        var result = await OnEndOfGameStats.Invoke(platformId, long.Parse(gameId));
        return result;
    }

    [HttpGet("getGameMetaData/{platformId}/{gameId}/{unknown}/token", Name = "GameMetaData")]
    public async Task<GameMetaData> GetGameMetaData(string platformId, string gameId, string unknown)
    {
        _logger.Log(LogLevel.Information, $"[getGameMetaData] Platform: {platformId} MatchId: {gameId} Unknown1: {unknown}");
        var result = await OnGetMetadata.Invoke(platformId, long.Parse(gameId), unknown);
        return result;
    }

    [HttpGet("getLastChunkInfo/{platformId}/{gameId}/{unknown}/token", Name = "LastChunkInfo")]
    public async Task<LastChunkInfo> GetLastChunkInfo(string platformId, string gameId, string unknown)
    {
        _logger.Log(LogLevel.Information,$"[getLastChunkInfo] Platform: {platformId} MatchId: {gameId} Unknown1: {unknown}");
        var result = await OnLasChunkInfo.Invoke(platformId, long.Parse(gameId), unknown);
        return result;
    }

    [HttpGet("getGameDataChunk/{platformId}/{gameId}/{chunkId}/token", Name = "GameDataChunk")]
    public async Task<byte[]> GetGameDataChunk(string platformId, string gameId, string chunkId)
    {
        _logger.Log(LogLevel.Information, $"[getGameDataChunk] Platform: {platformId} MatchId: {gameId} ChunkID: {chunkId}");
        var result = await OnGameDataChunk.Invoke(platformId, long.Parse(gameId), int.Parse(chunkId));
        return result;
    }

    [HttpGet("getKeyFrame/{platformId}/{gameId}/{frameId}/token", Name = "KeyFrame")]
    public async Task<byte[]> GetKeyFrame(string platformId, string gameId, string frameId)
    {
        _logger.Log(LogLevel.Information, $"[getKeyFrame] Platform: {platformId} MatchId: {gameId} FrameID: {frameId}");
        var result = await OnKeyFrame.Invoke(platformId, long.Parse(gameId), int.Parse(frameId));
        return result;
    }
}