using System.Text.Unicode;

namespace SpectatorAPI;

public static class Program
{
    private static readonly string Path = $"..\\bin\\Debug\\net9.0\\replay.lrf";
    public static void Main(string[] args)
    {
        var api = new ApiHandler();
        api.LoadReplay(Path);
        var key = Convert.ToBase64String(api.Replay.MetaData.EncryptionKey);
        Console.WriteLine(api.Replay.MetaData.Region);
        Console.WriteLine($"EncryptionKey: {api.Replay.MetaData.MatchId}");
        Console.WriteLine($"EncryptionKey: {key}");
        try
        {
            api.Start();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        Console.WriteLine("Press any key to exit...");
        Console.Read();
    }
}