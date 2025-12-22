namespace LeagueReplayFile.Structs;

public class BasicHeader
{
    public byte Unused { get; set; }
    public byte Version { get; set; }
    public byte Compressed { get; set; }
    public byte Reserved { get; set; }
}