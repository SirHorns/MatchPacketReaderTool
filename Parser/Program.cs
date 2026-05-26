using LeagueReplayFile;
using LeagueReplayFile.Enums;
using LeagueReplayFile.Models;
using LeagueReplayFileSerializer;
using LeagueReplayFileSerializer.Enums;

namespace Parser;

public static class Program
{
    private static readonly string SerializedDirectory;
    

    static Program()
    {
        SerializedDirectory = "Serialized";
        Directory.CreateDirectory(SerializedDirectory);
        foreach (var type in Enum.GetValues<ReplayType>())
        {
            Directory.CreateDirectory($"{SerializedDirectory}/{type}");
        }
    }
    
    public static void Main(string[] args)
    {
        Utils.Unhasher.Initialize();
        Console.WriteLine("<•······················•<>•······················•>");
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
            SingleReplay(path);
        }
        else
        {
            var lrfs = Directory.EnumerateFiles($"{path}\\", $"*.lrf", SearchOption.AllDirectories).ToList();
            if (lrfs.Count == 1)
            {
                SingleReplay(lrfs[0]);
            }
            else
            {
                var count = lrfs.Count;
                if (lrfs.Count == 0)
                {
                    Console.WriteLine($"No .lrf files found in \"{path}\"");
                }
                else
                {
                    BatchofReplays(lrfs);
                }
            }
        }

        Console.WriteLine("Done!");
        Exit();
    }

    private static void SingleReplay(string path)
    {
        Console.WriteLine("[Single Replays]");
        Console.WriteLine($"Reading {Path.GetFileName(path)}");
        LRF? lrf = ReadLRF(path);
        
        Console.WriteLine("Serialize replay?: [y/n]");
        var res = Console.ReadLine();
        if (res == null || !res.ToLowerInvariant().Equals("y"))
        {
            return;
        }
        
        Console.WriteLine("Unhash replay?: [y/n]");
        res = Console.ReadLine();
        bool result = false;
        if (res != null && res.ToLowerInvariant().Equals("y"))
        {
            result = true;
        }
        Console.WriteLine($"Serializing {Path.GetFileName(path)}");
        var slrf = lrf.SerializeLRF();
        
        Console.WriteLine("Write replay to file?: [y/n]");
        res = Console.ReadLine(); 
        result = false;
        if (res != null && res.ToLowerInvariant().Equals("y"))
        {
            result = true;
        }
        if (result)
        {
            slrf.WriteToFile(SerializedDirectory, $"{Path.GetFileNameWithoutExtension(path)}.slrf");
        }
    }

    public static void BatchofReplays(List<string> lrfPaths)
    {
        var i = 0; 
        var count = lrfPaths.Count;
        bool shouldSerialize = false;
        bool shouldUnhash = false;
        bool skipUnssuportedReplayVersions = false;
        
        Console.WriteLine($"[Batch of Replays]: {count} replays found.");
        
        
        Console.WriteLine("Serialize replays?: [y/n]");
        var result = Console.ReadLine();
        if (result != null && result.ToLowerInvariant().Equals("y"))
        {
            shouldSerialize = true;
        }
        if (shouldSerialize)
        {
            Console.WriteLine("Unhash replays?: [y/n]");
            result = Console.ReadLine();
            if (result != null && result.ToLowerInvariant().Equals("y"))
            {
                shouldUnhash = true;
            }
        }
        Console.WriteLine("Skip unsupported replay versions?: [y/n]");
        result = Console.ReadLine().ToLowerInvariant();
        switch (result)
        {
            case "y":
                skipUnssuportedReplayVersions = true;
                break;
            case "n":
                skipUnssuportedReplayVersions = false;
                break;
        }
        
        Console.WriteLine($"Replays will {(shouldSerialize ? "be" : "NOT be")} serialized.");
        if (shouldSerialize)
        {
            Console.WriteLine($"Replays will {(shouldUnhash ? "be" : "NOT be")} unhashed.");
        }
        Console.WriteLine($"Unsupported replays versions will {(skipUnssuportedReplayVersions ? "be" : "NOT be")} skipped.");
        
        List<LRF> lrfs = [];
        List<SLRF> slrfs = [];
        
        
        foreach (var lrfPath in lrfPaths)
        {
            Console.WriteLine("<•······················•<>•······················•>");
            ++i; 
            var rid = $"{i}/{count}";
            var md = ReadLRFMetaData(lrfPath, out var type);
            Console.WriteLine($"[{rid}]: {type} lrf");
            bool non420Replay;
            if (!md.ClientVersion.StartsWith("4.20"))
            {
                Console.Write($"{lrfPath} is not a 4.20.0.315 GameClient replay");
                if (skipUnssuportedReplayVersions)
                {
                    Console.WriteLine($"Skipping {lrfPath}");
                    continue;
                }
                Console.WriteLine("There could be issues parsing it. If any occur packets will not be read from any binary streams.");
            }

            LRF? lrf = ReadLRF(lrfPath);
            lrfs.Add(lrf);
        }

        if (shouldSerialize)
        {
            Console.WriteLine("Serialising Replays");
            foreach (var lrf in lrfs)
            {
                var slrf = lrf.SerializeLRF();
                slrfs.Add(slrf);
            }
        }
        if (shouldSerialize && shouldUnhash)
        {
            Console.WriteLine("Unhashing Replays");
            foreach (var slrf in slrfs)
            {
                slrf.UnhashSLRF();
            }
        } 

        List<Object> x;
        if (shouldSerialize) 
        {
            foreach (var slrf in slrfs)
            {
                slrf.WriteToFile(SerializedDirectory, $"{slrf.MetaData.MatchId}_{slrf.MetaData.Region}.slrf");
            }
        }
        else
        {
            foreach (var lrf in lrfs)
            {
                lrf.WriteToFile(SerializedDirectory, $"{lrf.MetaData.MatchId}_{lrf.MetaData.Region}.plrf");
            }
        }
        
        
    }
    
    
    public static ReplayMetaData? ReadLRFMetaData(string path, out LRFTypes type)
    {
        ReplayMetaData? metaData = null;
        try
        {
            var stream = File.OpenRead(path);
            var reader = new LRFReader();
            metaData = reader.ReadMetaData(stream, out type);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }

        return metaData;
    }

    private static LRF? ReadLRF(string path)
    {
        LRF? lrf = null;
        try
        {
            var stream = File.OpenRead(path);
            var reader = new LRFReader();
            lrf = reader.Read(stream);
            Console.WriteLine($"Client Version: {lrf.MetaData.ClientVersion}");
            Console.WriteLine($"Replay Version: {lrf.MetaData.ReplayVersion}");
            switch (lrf.Type)
            {
                case LRFTypes.NFO:
                    reader.ParseNFO();
                    break;
                case LRFTypes.HTTP:
                case LRFTypes.ENET:
                    reader.Parse();
                    break;
                case LRFTypes.NAN:
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return null;
        }

        return lrf;
    }
    
    //⬖•······················•⯁•······················•⬗
    
    private static void Exit() 
    { 
        Console.Write("Press any key to exit..."); 
        while (true)
        { 
            Console.Read(); 
            break;
        }
    }
}