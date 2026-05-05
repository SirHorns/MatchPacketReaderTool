namespace LeagueReplayFile.Models;

public class DataSegment
{
    public float Time { get; init; }
    public int Length { get; init; }
    public byte[] Data { get; init; }
    public byte Pad { get; init; }

    public static DataSegment Read(BinaryReader reader)
    {
        var time = reader.ReadSingle();
        var length = reader.ReadInt32();
        var segmentData = reader.ReadExactBytes(length);
        var padding = reader.ReadByte();
        
        var segment = new DataSegment()
        {
            Time = time,
            Length = length,
            Data = segmentData,
            Pad = padding
        };
        
        
        
        
        return segment;
    }

    public static void Write(DataSegment segment, BinaryWriter writer)
    {
        writer.Write(segment.Time);
        writer.Write(segment.Length);
        writer.Write(segment.Data);
        writer.Write(segment.Pad);
    }
}