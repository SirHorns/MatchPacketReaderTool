using LeagueReplayFile;
using LeagueReplayFile.Enums;
using LeagueReplayFile.Models;
using LeagueReplayFile.Models.Sections;
using LeagueReplayFileSerializer;
using Newtonsoft.Json;
using ReplayNamesUnhasher;
using SpectatorAPI.Controllers;

namespace SpectatorAPI;

public class APIServer
{
    private WebApplication app;
    private LRF replay;
    private static Unhasher Unhasher;

    private string Version = "";
    private GameMetaData GameMetaData;
    List<LastChunkInfo> LastChunkInfos = [];
    List<GameDataChunk> GameDataChunks = [];
    List<KeyFrame> KeyFrames = [];
    
    public APIServer()
    {
        Unhasher = new Unhasher();
        var builder = WebApplication.CreateBuilder();
// Add services to the container.
        builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        
        app = builder.Build();
// Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
        app.UseAuthorization();
        app.MapControllers();

        SpectatorReplayController.OnFeatured += () => Task.FromResult("{ }");
        SpectatorReplayController.OnVersion += () => Task.FromResult(Version);
        SpectatorReplayController.OnGetMetadata += () => Task.FromResult(GameMetaData);
        SpectatorReplayController.OnLasChunkInfo += (unk) => Task.FromResult<LastChunkInfo>(null);
        SpectatorReplayController.OnGameDataChunk += (id) =>
        {
            string res;
            foreach (var section in replay.Sections)
            {
                if (section is not GameDataSection dataSection)
                {
                    continue;
                }

                if (dataSection.ID != id)
                {
                    continue;
                }
            }
            return Task.FromResult<string>(null);
        };
        SpectatorReplayController.OnKeyFrame += (id) =>
        {
            return Task.FromResult<KeyFrame>(null);
        };
    }

    public void Run(string lrfPath)
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
        replay = lrf;
        foreach (var section in lrf.Sections)
        {
            string json;
            switch (section)
            {
                case GameDataSection gameDataSection:
                    break;
                case GameMetaDataSection gameMetaDataSection:
                    json = gameMetaDataSection.Json;
                    GameMetaData = JsonConvert.DeserializeObject<GameMetaData>(json);
                    break;
                case KeyFrameSection keyFrameSection:
                    break;
                case LastChunkInfoSection lastChunkInfoSection:
                     json = lastChunkInfoSection.Json;
                    var lci = JsonConvert.DeserializeObject<LastChunkInfo>(json);
                    LastChunkInfos.Add(lci);
                    break;
                case VersionSection versionSection:
                    Version = versionSection.Text;
                    break;
                default:
                    continue;
            }
        }
        app.Run();
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

    private static SLRF? SerializeLRF(LRF lrf)
    {
        SLRF? slrf = null;
        try
        {
            slrf = LRFSerializer.Serialize(lrf);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return slrf;
    }

    private static void UnhashSLRF(SLRF slrf)
    {
        try
        {
            Unhasher.Unhashie(slrf);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
}