namespace SpectatorAPI;

public class Program
{
    public static void Main(string[] args)
    {
        string path;
        if (args.Length == 0)
        {
            Console.WriteLine("Provide Path to lrf:");
            path = Console.ReadLine() ?? "";
        }
        else
        {
            path = args[0];
        }
        var api = new APIServer();
        try
        {
            api.Run(path);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
        Console.WriteLine("Press any key to exit...");
        Console.Read();
    }
}