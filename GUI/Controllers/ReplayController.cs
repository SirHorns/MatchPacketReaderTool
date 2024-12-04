using System;
using GUI.Services;
using LeaguePacketsSerializer.Objects;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace ReplayAPI.Controllers;

[ApiController]
[Route("/observer-mode/rest/consumer/")]
public class ReplayController: ControllerBase
{
    private readonly ILogger<ReplayController> _logger;

    public ReplayController(ILogger<ReplayController> logger)
    {
        _logger = logger;
    }
    
    [HttpGet("version", Name = "Version")]
    public string GetVersion()
    {
        var version = ReplayService.GetVersion();
        Console.WriteLine($"/observer-mode/rest/consumer/version");
        return version;
    }
    
    [HttpGet("getGameMetaData/{platformId}/{matchId}/{encodedEncryptionKey}/token", Name = "GameMetaData")]
    public string GetGameMetaData(string platformId, string matchId, string encodedEncryptionKey)
    {
        Console.WriteLine($"/observer-mode/rest/consumer/getGameMetaData/{platformId}/{matchId}/{encodedEncryptionKey}/token");
        return ReplayService.GetGameMetaData();
        /*
         return new GameMetaData()
        {
            GameKey = new GameKey()
            {
                GameId = 1611565207,
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
        };
         */
    }
    
    [HttpGet("getLastChunkInfo/{platformId}/{matchId}/{unknown1}/token", Name = "LastChunkInfo")]
    public string GetLastChunkInfo(string platformId, string matchId, string unknown1)
    {
        Console.WriteLine($"/observer-mode/rest/consumer/getLastChunkInfo/{platformId}/{matchId}/{unknown1}/token");
        var json = ReplayService.GetLastChunkInfo(platformId, matchId, unknown1);
        return json;
    }
    
    [HttpGet("getGameDataChunk/{platformId}/{matchId}/{chunkId}/token", Name = "GameDataChunk")]
    public GameDataChunk GetGameDataChunk(string platformId, string matchId, string chunkId)
    {
        Console.WriteLine($"/observer-mode/rest/consumer/getGameDataChunk/{platformId}/{matchId}/{chunkId}/token");
        return new GameDataChunk();
    }
    
    [HttpGet("getKeyFrame/{platformId}/{matchId}/{frameId}/token", Name = "KeyFrame")]
    public KeyFrame GetKeyFrame(string platformId, string matchId, string frameId)
    {
        Console.WriteLine($"/observer-mode/rest/consumer/getKeyFrame/{platformId}/{matchId}/{frameId}/token");
        return new KeyFrame();
    }
}