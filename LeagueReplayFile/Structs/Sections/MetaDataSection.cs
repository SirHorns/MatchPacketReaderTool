namespace LeagueReplayFile.Structs.Sections;

public class MetaDataSection : Section
{
    public int MatchId { get; internal set; }
    public string Json { get; internal set; }
}