using System.IO;

namespace LeaguePacketsSerializer.ReplayParser;

public class Chunk
{
    public float Time { get;  set; }
    public int Length { get; set; }
    public byte[] Data { get; set; }
    public byte Pad { get; set; }

    public static Chunk Read(BinaryReader chunksReader)
    {
        var t = chunksReader.ReadSingle();
        var l = chunksReader.ReadInt32();
        var d = chunksReader.ReadExactBytes(l);
        return new Chunk()
        {
            Time = t,
            Length = l,
            Data = d
        };
    }
}