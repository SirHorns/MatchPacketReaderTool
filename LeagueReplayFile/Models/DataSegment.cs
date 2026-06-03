namespace LeagueReplayFile.Models;

/// <summary>
/// Raw representation of data sent during a http request
/// </summary>
public class DataSegment
{
    public float Time { get; set; }
    public int Length  { get; set; }
    public byte[] Data { get; set; }
    public byte Pad { get; set; }
 
    public static DataSegment Read(BinaryReader reader)
    {
        DataSegment segment;
        var time = reader.ReadSingle();
        var length = reader.ReadInt32();
        var segmentData = reader.ReadExactBytes(length);
        var padding = reader.ReadByte();

        segment = new DataSegment()
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