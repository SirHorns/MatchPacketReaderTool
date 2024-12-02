namespace LeaguePacketsSerializer.Objects;

public class LastChunkInfo
{
    public int ChunkId { get; set; }
    public int AvailableSince { get; set; }
    public int NextAvailableChunk { get; set; }
    public int KeyFrameId { get; set; }
    public int NextChunkId { get; set; }
    public int EndStartupChunkId { get; set; }
    public int StartGameChunkId { get; set; }
    public int EndGameChunkId { get; set; }
    public int Duration { get; set; }
}