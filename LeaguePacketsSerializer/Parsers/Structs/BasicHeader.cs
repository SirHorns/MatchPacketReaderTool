using System.IO;

namespace LeaguePacketsSerializer.Parsers;

public class BasicHeader
{
    public byte Unused { get; set; }
    public byte Version { get; set; }
    public byte Compressed { get; set; }
    public byte Reserved { get; set; }

    public static BasicHeader Read(BinaryReader reader)
    {
        return new BasicHeader()
        {
            Unused = reader.ReadByte(),
            Version = reader.ReadByte(),
            Compressed = reader.ReadByte(),
            Reserved = reader.ReadByte()
        };
    }
}