namespace LeagueReplayFile.Models;

public class EndOfGameStats
{
    public bool Ranked{ get; set; }
    public string GameType{ get; set; }
    public object TeamPlayerParticipantStats{ get; set; }
    public long GameId{ get; set; }
    public object Difficulty{ get; set; }
    public long GameLength{ get; set; }
    public string DameMode{ get; set; }
    public object MyTeamInfo{ get; set; }
    public object Invalid{ get; set; }
    public object OtherTeamInfo{ get; set; }
    public object QueueType{ get; set; }
    public object MyTeamStatus{ get; set; }
    public object OtherTeamPlayerParticipantStats{ get; set; }
    public string _class{ get; set; }
}