using System.IO;

namespace LeaguePacketsSerializer.Parsers;

public class DataSegment
{
    public float Time { get; private init; }
    public int Length { get; private init; }
    public byte[] Data { get; private init; }
    public byte Pad { get; private init; }

    public static DataSegment Read(BinaryReader reader)
    {
        var t = reader.ReadSingle();
        var l = reader.ReadInt32();
        var d = reader.ReadExactBytes(l);
        var p = reader.ReadByte();
        
        return new DataSegment()
        {
            Time = t,
            Length = l,
            Data = d,
            Pad = p
        };
    }
}