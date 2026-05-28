namespace LeagueReplayFile.Models;

public class Player
{
    public string Summoner { get; set; }
    public string Champion { get; set; }
    public int Kills { get; set; }
    public int Level { get; set; }
    public int Deaths { get; set; }
    public int Assists { get; set; }
    public int Team { get; set; }
    public bool Won { get; set; }
    public int NodeNeutralizes { get; set; }
    public long AccountID { get; set; }
}