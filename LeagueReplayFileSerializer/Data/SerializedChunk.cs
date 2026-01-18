namespace LeagueReplayFileSerializer.Data;

public class SerializedChunk
{
    public int ID { get; set; }
    public List<SerializedPacket> Packets { get; set; } = [];
}