using System.IO.Compression;

namespace LeagueReplayFile.Models.Sections;

public abstract class StreamSection: Section
{
    public static MemoryStream Compress(BlowFish blowfish, byte[] data)
    {
        var decrypted = blowfish.Decrypt(data);
        var compressed = new MemoryStream();
        using var decompressed = new GZipStream(new MemoryStream(decrypted), CompressionMode.Compress);
        decompressed.CopyTo(compressed);
        return compressed;
    }
    
    public static MemoryStream Decompress(BlowFish blowfish, byte[] data)
    {
        var decrypted = blowfish.Decrypt(data);
        var decompressed = new MemoryStream();
        using var compressed = new GZipStream(new MemoryStream(decrypted), CompressionMode.Decompress);
        compressed.CopyTo(decompressed);
        return decompressed;
    }
}