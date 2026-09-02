using LeagueReplayFile.Models.Sections;

namespace LeagueReplayFileSerializer.Data;

public class SerializedGameDataSection : SerializedSection
{
    public int ID { get; internal set; }
    public SerializedChunk Chunk { get; internal set; } = new SerializedChunk();
}