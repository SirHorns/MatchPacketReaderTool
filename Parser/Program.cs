using LeagueReplayFile;
using LeagueReplayFileSerializer;
using LeagueReplayFileSerializer.Enums;
using Newtonsoft.Json;
using ReplayNamesUnhasher;

namespace Parser;

public static class Program
{
    private const string SerializedDirectory = "Serialized";

    public static void Main(string[] args)
    {
        Directory.CreateDirectory(SerializedDirectory);
        foreach (var type in Enum.GetValues<ReplayType>())
        {
            Directory.CreateDirectory($"{SerializedDirectory}/{type}");
        }

        string path;
        if (args.Length == 0)
        {
            Console.WriteLine("Provide Path:");
            path = Console.ReadLine() ?? "";
        }
        else
        {
            path = args[0];
        }
        var dirs = Directory.EnumerateFiles($"{path}\\", $"*.lrf", SearchOption.AllDirectories);
        var count = dirs.Count();
        if (dirs.Count() == 0)
        {
            Console.WriteLine($"No .lrf files found in \"{path}\"");
            Exit();
            return;
        }
        
        Console.WriteLine($"Total replays found: {count}");
        
        var i = 0;
        var unhasher = new Unhasher();
        unhasher.Initialize();

        foreach (var lrfPath in dirs)
        {
            ++i;
            var fileName = $"{Path.GetFileNameWithoutExtension(lrfPath)}.slrf";
            if (File.Exists($"{SerializedDirectory}/{fileName}"))
            {
                Console.WriteLine($"Skipping {fileName}; Already exists.");
            }
            var rid = $"{i}/{count}";
            try
            {
                var stream = File.OpenRead(lrfPath);
                var reader = new LRFReader();
                var lrf = reader.Read(stream);
                Console.WriteLine($"[{rid}]: {lrf.Type}");
                var serializer = new LRFSerializer();
                var slrf = serializer.CreateSerializedLRF(lrf);
                unhasher.Unhashie(slrf);
        
        
                var json = JsonConvert.SerializeObject(slrf, Formatting.Indented);
                File.WriteAllText($"{SerializedDirectory}/{slrf.Type}/{fileName}", json);
            }
            catch (Exception e)
            {
                Console.WriteLine($"[{rid}]: Error - {e}");
            }
        }

        Console.WriteLine("Done");
        Exit();
    }


    private static void Exit()
    {
        Console.Write("Press any key to exit...");
        Console.Read();
    }
}