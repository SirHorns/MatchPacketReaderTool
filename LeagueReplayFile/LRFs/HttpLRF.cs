using LeagueReplayFile.Maestro;
using LeagueReplayFile.Models.Http;
using LeagueReplayFile.Models.Sections;

namespace LeagueReplayFile.LRFs;

public class HttpLRF : LRF
{
    /// <summary>
    /// Version from http stream
    /// </summary>
    public string Version { get;  internal set; } = "";
    /// <summary>
    /// GameMetaData from http stream
    /// </summary>
    public GameMetaData? GameMetaData { get;  internal set; }
    /// <summary>
    /// Represents Http requests and their following data
    /// </summary>
    public List<Section> Sections { get; internal set; } = [];

    public List<MaestroMessage> Messages { get; internal set; } = [];
    

    public List<LastChunkInfoSection> GetLastChunkInfoSections()
    {
        List<LastChunkInfoSection> sections = [];
        foreach (var section in Sections)
        {
            if (section is LastChunkInfoSection lastChunkInfoSection)
            {
                sections.Add(lastChunkInfoSection);
            }
        }
        return sections;
    }
    
    public List<KeyFrameSection> GetKeyFrameSections()
    {
        List<KeyFrameSection> sections = [];
        foreach (var section in Sections)
        {
            if (section is KeyFrameSection keyFrameSection)
            {
                sections.Add(keyFrameSection);
            }
        }
        return sections;
    }
    
    public List<GameDataChunkSection> GetGameDataChunkSections()
    {
        List<GameDataChunkSection> sections = [];
        foreach (var section in Sections)
        {
            if (section is GameDataChunkSection gameDataChunkSection)
            {
                sections.Add(gameDataChunkSection);
            }
        }
        return sections;
    }
    
}