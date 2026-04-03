namespace SpectatorAPI;

public static class Program
{
    private static readonly string Path = $"..\\bin\\Debug\\net9.0\\replay.lrf";
    public static void Main(string[] args)
    {
        var api = new ReplayApiServer();
        api.LoadReplay(Path);
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