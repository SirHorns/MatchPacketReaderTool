using LeagueReplayFile.Models.Http;
using LeagueReplayFile.Protocols.ENet;

namespace LeagueReplayFile.Models.Sections;

public class GameDataChunkSection : Section, IStreamSection
{
    public int ID { get; internal set; }
    public long GameId { get; set; }
    public Chunk Chunk { get; internal set; } = new Chunk();
    
    public void Write(BinaryWriter writer)
    {
        Chunk.Write(writer);
    }
}