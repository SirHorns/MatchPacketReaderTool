namespace LeagueReplayFile.Models;

public class DataSegment
{
    public float Time { get; init; }
    public int Length { get; init; }
    public byte[] Data { get; init; }
    public byte Pad { get; init; }
}