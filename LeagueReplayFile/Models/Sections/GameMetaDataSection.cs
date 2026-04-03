namespace LeagueReplayFile.Models.Sections;

public class GameMetaDataSection : Section
{
    public int GameId { get; internal set; }
    public string Json { get; internal set; }
}