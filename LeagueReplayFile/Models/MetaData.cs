namespace LeagueReplayFile.Models;

/// <summary>
/// Metadata about the replay
/// </summary>
public class MetaData
{
    public Screenshot[] Screenshots { get; set; }
    public int StartGameChunkId{ get; set; }
    public int Map { get; set; }
    public int QueueId{ get; set; }
    public int FirstWinBonus { get; set; }
    public List<KeyValuePair<string, DataOffset>> DataIndex { get; set; }
    public Player[] Players { get; set; }
    public bool IsStream{ get; set; }
    public int StatsVersion { get; set; }
    public long MatchId { get; set; }
    public string ClientHash { get; set; }
    public int GameMode { get; set; }
    public byte[] EncryptionKey { get; set; }
    public long MatchLength { get; set; }
    public bool ObserverStream { get; set; }
    public string Region { get; set; }
    public ushort ServerPort { get; set; }
    public Dictionary<string, object> Masteries;
    public int WinningTeam { get; set; }
    public string ReplayVersion { get; set; }
    public long TimeStamp { get; set; }
    public long AccountId { get; set; }
    public object[] Teams { get; set; } = [];
    public bool SpectatorMode { get; set; }
    public bool Ranked { get; set; }
    public string ClientVersion { get; set; }
    public string Name { get; set; }
    public string QueueType { get; set; } = "N/A";
    public int MatchType { get; set; }
    public Dictionary<string, object> Runes { get; set; }
    public long ReplayId { get; set; }
    public int EndStartupChunkId{ get; set; }
    public string ServerAddress { get; set; }
    public string SummonerName { get; set; }
}