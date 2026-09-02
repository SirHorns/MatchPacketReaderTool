using LeagueReplayFile;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Inventory;

public static class Searches
{
    public static void FindChampionReplays(string directory, List<string> lrfs, string champion)
    {
        Console.WriteLine($"Looking for {champion} replays...");

        var i = 0;
        JArray jar = [];
        foreach (var lrfPath in lrfs)
        {
            var fileName = $"{Path.GetFileName(lrfPath)}";
            var stream = File.OpenRead(lrfPath);
            var metaData = LRFReader.Read(stream, out var type);
            foreach (var player in metaData.Players)
            {
                if (player.Champion.ToLowerInvariant().Equals(champion.ToLowerInvariant()))
                {
                    i++;
                    Console.WriteLine($"[{i}]: {lrfPath}");
                    var heros = metaData.Players.Select(p => p.Champion.ToLowerInvariant());
                    Console.WriteLine($"- [{string.Join(", ", heros)}]");
                    object[] props = 
                    [
                        new JProperty("index", i), 
                        new JProperty("path", lrfPath), 
                        new JProperty("name", fileName), 
                        new JProperty("heros", heros)
                    ];
                    jar.Add(new JObject(props));
                }
            }
        }

        Console.WriteLine($"Total {champion} replays found: {i}");
        var json = JsonConvert.SerializeObject(jar, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        File.WriteAllText($"{directory}\\{champion}_replays.json", json);
    }

    public static void FindSpectatorReplays(string directory, List<string> lrfs)
    {
        Console.WriteLine($"Looking for spectator replays...");
        
        var i = 0;
        JArray jar = [];

        foreach (var lrfPath in lrfs)
        {
            var fileName = $"{Path.GetFileName(lrfPath)}";
            var stream = File.OpenRead(lrfPath);
            var metaData = LRFReader.Read(stream, out var type);
            if (metaData.SpectatorMode)
            {
                i++;
                Console.WriteLine($"[{i}]: {lrfPath}");
                object[] props = 
                [
                    new JProperty("index", i), 
                    new JProperty("path", lrfPath), 
                    new JProperty("name", fileName)
                ];
                jar.Add(new JObject(props));
            }
        }

        Console.WriteLine($"Total spectator replays found: {i}");
        var json = JsonConvert.SerializeObject(jar, Formatting.Indented, new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
        var filepath = $"{directory}\\spectator_replays.json";
        Console.WriteLine($"Writing to: {filepath}");
        File.WriteAllText(filepath, json);
    }
}