using System.IO.Compression;
using LeagueReplayFile.Models;

namespace LeagueReplayFile;

public static class BinaryDataExtensions
{
    public static MemoryStream CompressToMemoryStream(this BlowFish blowfish, byte[] data)
    {
        var encrypted = blowfish.Encrypt(data);
        var outputStream = new MemoryStream();
        using var gzipStream = new GZipStream(outputStream, CompressionMode.Compress);
        gzipStream.Write(encrypted, 0, encrypted.Length);
        return outputStream;
    }
    
    public static MemoryStream DecompressToMemoryStream(this BlowFish blowfish, byte[] data)
    {
        var decrypted = blowfish.Decrypt(data);
        var result = new MemoryStream();
        using var decompressed = new GZipStream(new MemoryStream(decrypted), CompressionMode.Decompress);
        decompressed.CopyTo(result);
        result.Seek(0, SeekOrigin.Begin);
        return result;
    }
    
    public static byte[] CompressToByteArray(this BlowFish blowfish, byte[] data)
    {
        var encrypted = blowfish.Encrypt(data);
        using var gzipStream = new GZipStream(new MemoryStream(encrypted), CompressionMode.Compress);
        var output = new byte[encrypted.Length];
        gzipStream.Write(output, 0, encrypted.Length);
        return output;
    }
    
    public static byte[] DecompressToByteArray(this BlowFish blowfish, byte[] data)
    {
        var decrypted = blowfish.Decrypt(data);
        using var decompressed = new GZipStream(new MemoryStream(decrypted), CompressionMode.Decompress);
        var output = new byte[decrypted.Length];
        decompressed.Write(output, 0, decrypted.Length);
        return output;
    }
    
    public static byte[] Compress(this byte[] data)
    {
        using var stream = new MemoryStream(data);
        using var compressed = new GZipStream(stream, CompressionMode.Compress);
        var output = new byte[compressed.Length];
        compressed.Write(output, 0, output.Length);
        return output.ToArray();
    }
    
    public static byte[] Decompress(this byte[] data)
    {
        var decompressed = new MemoryStream();
        using var compressed = new GZipStream(new MemoryStream(data), CompressionMode.Decompress);
        compressed.CopyTo(decompressed);
        return decompressed.ToArray();
    }
}