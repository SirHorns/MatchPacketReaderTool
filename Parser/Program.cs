using LeagueReplayFile;
using LeagueReplayFileSerializer;
using LeagueReplayFileSerializer.Enums;
using Newtonsoft.Json;
using ReplayNamesUnhasher;

namespace Parser;

public static class Program
{
    private static string SerializedDirectory;
    private static Unhasher Unhasher;

    static Program()
    {
        SerializedDirectory = "Serialized";
        Unhasher = new Unhasher();
        Directory.CreateDirectory(SerializedDirectory);
        foreach (var type in Enum.GetValues<ReplayType>())
        {
            Directory.CreateDirectory($"{SerializedDirectory}/{type}");
        }
    }
    
    public static void Main(string[] args)
    {
        Unhasher.Initialize();
        string path;
        if (args.Length == 0)
        {
            Console.WriteLine("Provide Path to lrf(s):");
            path = Console.ReadLine() ?? "";
        }
        else
        {
            path = args[0];
        }
        
        
        if(path.EndsWith(".lrf"))
        {
            ParseReplay(path);
        }
        else
        {
            var lrfPaths = GetFilePaths(path);
            ParseReplays(lrfPaths);
        }

        Console.WriteLine("Done");
        Exit();
    }

    private static LRF? ReadLRF(string path)
    {
        LRF? lrf = null;
        try
        {
            var stream = File.OpenRead(path);
            var reader = new LRFReader();
            lrf = reader.Read(stream);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return lrf;
    }

    private static SLRF? SerializeLRF(LRF lrf)
    {
        SLRF? slrf = null;
        var serializer = new LRFSerializer();
        try
        {
            slrf = serializer.CreateSerializedLRF(lrf);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return slrf;
    }

    private static void UnhashSLRF(SLRF slrf)
    {
        try
        {
            Unhasher.Unhashie(slrf);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    private static void WriteToFile(SLRF slrf, string fileName)
    {
        var json = JsonConvert.SerializeObject(slrf, Formatting.Indented);
        File.WriteAllText($"{SerializedDirectory}/{slrf.Type}/{fileName}", json);   
    }

    private static void ParseReplay(string lrfPath)
    {
        var fileName = $"{Path.GetFileNameWithoutExtension(lrfPath)}.slrf";
        if (File.Exists($"{SerializedDirectory}/{fileName}"))
        {
            Console.WriteLine($"Skipping {fileName}; Already exists.");
            return;
        }
        var lrf = ReadLRF(lrfPath);
        Console.WriteLine($"{lrf.Type}");
        var slrf = SerializeLRF(lrf);
        UnhashSLRF(slrf);
        //WriteToFile(slrf, fileName);
    }

    private static void ParseReplays(List<string> lrfPaths)
    {
        var i = 0;
        var count = lrfPaths.Count;
        foreach (var lrfPath in lrfPaths)
        {
            ++i; 
            var fileName = $"{Path.GetFileNameWithoutExtension(lrfPath)}.slrf";
            if (File.Exists($"{SerializedDirectory}/{fileName}"))
            {
                Console.WriteLine($"Skipping {fileName}; Already exists.");
                continue;
            }
            var lrf = ReadLRF(lrfPath);
            var rid = $"{i}/{count}";
            Console.WriteLine($"[{rid}]: {lrf.Type}");
            var slrf = SerializeLRF(lrf);
            UnhashSLRF(slrf);
            //WriteToFile(slrf, fileName);
        }
    }
    

    private static List<string>? GetFilePaths(string path)
    {
        var lrfs = Directory.EnumerateFiles($"{path}\\", $"*.lrf", SearchOption.AllDirectories).ToList();
        var count = lrfs.Count;
        if (lrfs.Count == 0)
        {
            Console.WriteLine($"No .lrf files found in \"{path}\"");
            Exit();
            return null;
        }
        
        Console.WriteLine($"Total replays found: {count}");
        return lrfs;
    }


    private static void Exit()
    {
        Console.Write("Press any key to exit...");
        Console.Read();
    }
}