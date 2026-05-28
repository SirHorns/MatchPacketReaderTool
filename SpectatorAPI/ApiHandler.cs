using LeagueReplayFile;
using LeagueReplayFile.Enums;
using LeagueReplayFile.Models;
using LeagueReplayFile.Models.Sections;
using Newtonsoft.Json;
using SpectatorAPI.Controllers;

namespace SpectatorAPI;

public class ApiHandler
{
    private bool _started;
    
    private WebApplication WebApp;
    public LRF Replay;
    public string Version = "";
    public MetaData GameMetaData;
    private List<LastChunkInfoSection> LastChunkInfos;
    private Dictionary<int, GameDataChunkSection> GameDatas;
    private Dictionary<int, KeyFrameSection> KeyFrames;
    private int Index { get; set; } = 0;

    public ApiHandler()
    {
        _started = false;
        GameMetaData = new MetaData();
        LastChunkInfos = [];
        GameDatas = [];
        KeyFrames = [];
        var builder = WebApplication.CreateBuilder();
// Add services to the container.
        builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        
        WebApp = builder.Build();
// Configure the HTTP request pipeline.
        if (WebApp.Environment.IsDevelopment())
        {
            WebApp.MapOpenApi();
        }
        WebApp.UseAuthorization();
        WebApp.MapControllers();

        SpectatorReplayController.OnFeatured += GetFeatured;
        SpectatorReplayController.OnVersion += GetVersion;
        SpectatorReplayController.OnEndOfGameStats += GetEndOfGameStats;
        SpectatorReplayController.OnGetMetadata += GetMetaData;
        SpectatorReplayController.OnLasChunkInfo += GetLastChunkInfo;
        SpectatorReplayController.OnGameDataChunk += GetGameDataChunk;
        SpectatorReplayController.OnKeyFrame += GetKeyFrame;
    }

    public void Start()
    {
        if (_started)
        {
            return;
        }
        _started = true;
        WebApp.Run();
    }

    public void Stop()
    {
        if (!_started)
        {
            return;
        }
        _started = false;
        WebApp.StopAsync();
    }
    
    //<•······················•<>•······················•>

    public object GetPlatform(string id)
    {
        return default;
    }

    public object GetGame(long id)
    {
        return default;
    }
    
    //<•······················•<>•······················•>
    
    private Task<string> GetFeatured()
    {
        return Task.FromResult("{ }");
    }
    
    private Task<string> GetVersion()
    {
        return Task.FromResult(Version);
    }
    
    private Task<EndOfGameStats> GetEndOfGameStats(string platformId, long gameId)
    {
        var platform = GetPlatform(platformId);
        var game = GetGame(gameId);
        return Task.FromResult(new EndOfGameStats());
    }
    
    private Task<MetaData> GetMetaData(string platformId, long gameId, string unknown)
    {
        var platform = GetPlatform(platformId);
        var game = GetGame(gameId);
        return Task.FromResult(GameMetaData);
    }

    
    private Task<LastChunkInfo> GetLastChunkInfo(string platformId, long gameId, string unknown)
    {
        var platform = GetPlatform(platformId);
        var game = GetGame(gameId);
        Index++;
        if (Index> LastChunkInfos.Count)
        {
            Console.WriteLine($"OUT OF RANGE: {Index} > {LastChunkInfos.Count}");
            return null;
        }
        var section = LastChunkInfos[Index];
        var json = section.Json;
        var info = JsonConvert.DeserializeObject<LastChunkInfo>(json);
        return Task.FromResult(info);
    }
    
    //TODO: Implement Compressing Stream
    private Task<byte[]> GetGameDataChunk(string platformId, long gameId, int chunkId)
    {
        var platform = GetPlatform(platformId);
        var game = GetGame(gameId);
        if (!GameDatas.TryGetValue(chunkId, out var section))
        {
            return Task.FromResult(Array.Empty<byte>());
        }

        var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        section.Write(writer);
        return Task.FromResult(stream.ToArray());
    }
    
    //TODO: Implement Compressing Stream
    private Task<byte[]> GetKeyFrame(string platformId, long gameId, int frameId)
    {
        var platform = GetPlatform(platformId);
        var game = GetGame(gameId);
        if (!KeyFrames.TryGetValue(frameId, out var section))
        {
            return Task.FromResult(Array.Empty<byte>());
        }
        
        var stream = new MemoryStream();
        using var writer = new BinaryWriter(stream);
        section.Write(writer);
        
        return Task.FromResult(stream.ToArray());
    }
    
    //<•······················•<>•······················•>

    public void LoadReplay(string lrfPath)
    {
        var lrf = ReadLRF(lrfPath);
        switch (lrf.Type)
        {
            case LRFTypes.HTTP:
                break;
            case LRFTypes.NAN:
                throw new NotImplementedException($"Replay is invalid");
            case LRFTypes.NFO:
            case LRFTypes.ENET:
                throw new NotImplementedException($"{lrf.Type} replay is not implemented yet");
            default:
                throw new ArgumentOutOfRangeException();
        }
        Replay = lrf;
        ParseSections();
    }
    
    private static LRF? ReadLRF(string path)
    {
        LRF? lrf = null;
        try
        {
            var stream = File.OpenRead(path);
            var reader = new LRFReader();
            lrf = reader.Read(stream);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        return lrf;
    }
    
    private void ParseSections()
    {
        Console.WriteLine("Parsing Sections...");
        foreach (var section in Replay.Sections)
        {
            string json;
            switch (section)
            {
                case GameDataChunkSection gameDataSection:
                    if (!GameDatas.TryAdd(gameDataSection.ID, gameDataSection))
                    {
                        //Console.WriteLine($"Duplicate GameDataSection ID: {gameDataSection.ID}");
                    }
                    break;
                case GameMetaDataSection gameMetaDataSection:
                    json = gameMetaDataSection.Json;
                    var metaData = JsonConvert.DeserializeObject<MetaData>(json);
                    GameMetaData = metaData ?? throw new NullReferenceException("Game MetaData is null!!!");
                    break;
                case KeyFrameSection keyFrameSection:
                    if (!KeyFrames.TryAdd(keyFrameSection.ID, keyFrameSection))
                    {
                        //Console.WriteLine($"Duplicate KeyFrameSection ID: {keyFrameSection.ID}");
                    }
                    break;
                case LastChunkInfoSection lastChunkInfoSection:
                    LastChunkInfos.Add(lastChunkInfoSection);
                    break;
                case VersionSection versionSection:
                    Version = versionSection.Text;
                    break;
                default:
                    continue;
            }
        }
    }
}