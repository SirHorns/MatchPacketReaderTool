using LeagueReplayFile.Protocols.ENet;

namespace LeagueReplayFile.Models.Sections;

public class GameDataSection : StreamSection
{
    public int ID { get; internal set; }
    public long GameId { get; set; }
    public GameDataChunk Chunk { get; internal set; } = new GameDataChunk();

    
    public void Write(BinaryWriter writer)
    {
        Chunk.Write(writer);
    }
}