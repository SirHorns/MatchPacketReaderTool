using System.IO;

namespace LeaguePacketsSerializer.Parsers;

public class DataSegment
{
    public float Time { get; private init; }
    public int Length { get; private init; }
    public byte[] Data { get; private init; }
    public byte Pad { get; private init; }

    public static DataSegment Read(BinaryReader chunksReader)
    {
        var t = chunksReader.ReadSingle();
        var l = chunksReader.ReadInt32();
        var d = chunksReader.ReadExactBytes(l);
        var p = chunksReader.ReadByte();
        
        return new DataSegment()
        {
            Time = t,
            Length = l,
            Data = d,
            Pad = p
        };
    }
}