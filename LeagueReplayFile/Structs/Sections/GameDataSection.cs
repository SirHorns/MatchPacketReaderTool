namespace LeagueReplayFile.Structs.Sections;

public class GameDataSection : Section
{
    public int ID { get; internal set; }
    public Chunk Chunk { get; internal set; } = new Chunk();
}