namespace LeagueReplayFile.Models.Sections;

public class GameDataSection : Section
{
    public int ID { get; internal set; }
    public int GameId { get; set; }
    public GameDataChunk Chunk { get; internal set; } = new GameDataChunk();
}