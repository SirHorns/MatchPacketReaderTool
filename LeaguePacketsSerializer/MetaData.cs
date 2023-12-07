namespace LeaguePacketsSerializer;

public class MetaData
{
    //public QueueType
    public string ReplayVersion;
    public int GameMode;
    public ushort ServerPort;
    public Screenshot[] Screenshots;
    public string ClientVersion;
    public int MatchType;
    public bool ObserverStream;
    public long AccountId;
    public int Map;
    public string SummonerName;
    public long MatchLength;
    public long TimeStamp;
    public bool Ranked;
    public Index[] DataIndex;
    public string ServerAddress;
    public int WinningTeam;
    public bool SpectatorMode;
    public string Name;
    public int FirstWinBonus;
    public string Region;
    public long MatchId;
    public long ReplayId;
    //public Teams;
    public Player[] Players;
    public int StatsVersion;
    public string EncryptionKey;
    public string ClientHash;
}

public class Screenshot
{
    public long TimeStamp;
    public string Type;
    public string Name;
}

public class Index
{
    public IndexValue Value;
    public string Key;

    public class IndexValue
    {
        public long Size;
        public long Offset;
    }
}

public class Player
{
    public string Summoner;
    public string Champion;
    public int Kills;
    public int Level;
    public int Deaths;
    public int Assists;
    public int Team;
    public bool Won;
    public int NodeNeutralizes;
    public int AccountID;
}