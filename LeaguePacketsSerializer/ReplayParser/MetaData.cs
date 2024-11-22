using System.Collections.Generic;
using Newtonsoft.Json;

namespace LeaguePacketsSerializer.ReplayParser;

public class MetaData
{
    public string QueueType { get; set; } = "N/A";
    public string ReplayVersion { get; set; }
    public int GameMode { get; set; }
    public ushort ServerPort { get; set; }
    public Screenshot[] Screenshots { get; set; }
    public string ClientVersion { get; set; }
    public int MatchType { get; set; }
    public bool ObserverStream { get; set; }
    public long AccountId { get; set; }
    public int Map { get; set; }
    public string SummonerName { get; set; }
    public long MatchLength { get; set; }
    public long TimeStamp { get; set; }
    public bool Ranked { get; set; }
    public List<KeyValuePair<string, DataOffset>> DataIndex { get; set; }
    public string ServerAddress { get; set; }
    public int WinningTeam { get; set; }
    public bool SpectatorMode { get; set; }
    public string Name { get; set; }
    public int FirstWinBonus { get; set; }
    public string Region { get; set; }
    public int MatchId { get; set; }

    public long ReplayId { get; set; }

    public string Teams { get; set; } = "N/A";
    public Player[] Players { get; set; }
    public int StatsVersion { get; set; }
    public byte[] EncryptionKey { get; set; }
    public string ClientHash { get; set; }
}

public class Screenshot
{
    public long TimeStamp { get; set; }
    public string Type { get; set; }
    public string Name { get; set; }
}

public class Index
{
    [JsonProperty("Value")]
    public IndexValue Value { get; set; }
    public string Key { get; set; }

    
    public class IndexValue
    {
        public long Size { get; set; }
        public long Offset { get; set; }
    }
}

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
    public int AccountID { get; set; }
}