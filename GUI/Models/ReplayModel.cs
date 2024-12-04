using LeaguePacketsSerializer;
using LeaguePacketsSerializer.Parsers;

namespace GUI.Models;

public class ReplayModel
{
    public Replay Replay { get; set; }
    public string FilePath { get; set; }

    public ReplayInfo Info => Replay.ReplayInfo;
}