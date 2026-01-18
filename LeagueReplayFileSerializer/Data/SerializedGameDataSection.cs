using LeagueReplayFile.Structs.Sections;

namespace LeagueReplayFileSerializer.Data;

public class SerializedGameDataSection : Section
{
    public int ID { get; internal set; }
    public SerializedChunk Chunk { get; internal set; } = new SerializedChunk();
}