using LeagueReplayFile;
using LeagueReplayFileSerializer;
using Newtonsoft.Json;
using ReplayNamesUnhasher;

namespace Parser;

public static class Utils
{
    public static readonly Unhasher Unhasher;

    static Utils()
    {
        Unhasher = new Unhasher();
    }
    
    public static SLRF? SerializeLRF(this LRF lrf)
    {
        SLRF? slrf = null;
        try
        {
            slrf = LRFSerializer.Serialize(lrf);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }

        return slrf;
    }
    
    public static void UnhashSLRF(this SLRF slrf)
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
    
    public static void WriteToFile(this LRF lrf, string directory, string fileName) => Write(lrf, directory, fileName);
    public static void WriteToFile(this SLRF slrf, string directory, string fileName) => Write(slrf, directory, fileName);

    private static void Write(object replay, string directory, string fileName)
    {
        if (File.Exists($"{directory}/{fileName}"))
        {
            Console.WriteLine($"Skipping {fileName}; Already exists.");
            return;
        }
        var path = $"{directory}/{fileName}";
        Console.WriteLine($"Outputted SLRF to {path}");
        var json = JsonConvert.SerializeObject(replay, Formatting.Indented);
        File.WriteAllText(path, json);   
    }
    
    //⬖•······················•⯁•······················•⬗
    
    public static List<SLRF> SerializeLRFs(this List<LRF> lrfs)
    {
        Console.WriteLine("Serialising Replays");
        List<SLRF> slrfs = [];
        foreach (var lrf in lrfs)
        {
            var slrf =  lrf.SerializeLRF();
            slrfs.Add(slrf);
        }
        return slrfs;
    }
    
    public static void UnhashSLRFs(this List<SLRF> slrfs)
    {
        Console.WriteLine("Unhashing Replays");
        foreach (var slrf in slrfs)
        {
            try
            {
                Utils.Unhasher.Unhashie(slrf);
                Utils.Unhasher.Reset();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
}