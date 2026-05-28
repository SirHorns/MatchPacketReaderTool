namespace LeagueReplayFile.Enums;

public enum LRFType
{
    /// <summary>
    /// Unable to determine the type of replay
    /// </summary>
    NAN,
    /// <summary>
    /// Currently unknown what a NFO replay is
    /// </summary>
    NFO,
    /// <summary>
    /// This replay sends data over HTTP connections
    /// </summary>
    HTTP,
    /// <summary>
    /// This replay is just a stream of ENet packets
    /// </summary>
    ENET
}