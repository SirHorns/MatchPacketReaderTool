// See https://aka.ms/new-console-template for more information

using System.Text.Json.Serialization;
using LeagueReplayFile;
using LeagueReplayFileSerializer.Enums;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

Console.WriteLine("Hello!");

string directory;
if (args.Length > 0)
{
    directory = args[0];
}
else
{
    Console.WriteLine("Provide a directory that contains replays:");
    directory = Console.ReadLine();
}

if (string.IsNullOrEmpty(directory) || !Directory.Exists(directory))
{
    Console.WriteLine($"Invalid Directory: \"{directory}\"");
    Console.WriteLine("Press any key to exit...");
    Console.Read();
    return;
}

var lrfs = Directory.EnumerateFiles($"{directory}\\", $"*.lrf", SearchOption.AllDirectories).ToList();
var count = lrfs.Count;
if (lrfs.Count == 0)
{
    Console.WriteLine($"No .lrf files found in: {directory}");
    Console.WriteLine("Press any key to exit...");
    Console.Read();
    return;
}
Console.WriteLine($"Total replays found: {count}");

LRFReader reader;

Console.WriteLine($"Looking for {args[1]} replays...");

var i = 0;
JArray jar = [];
foreach (var lrfPath in lrfs)
{
    var fileName = $"{Path.GetFileName(lrfPath)}";
    var stream = File.OpenRead(lrfPath);
    reader = new LRFReader();
    var metaData = reader.ReadMetaData(stream, out var type);
    reader.Dispose();
    foreach (var player in metaData.Players)
    {
        if (player.Champion.ToLowerInvariant().Equals(args[1].ToLowerInvariant()))
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

Console.WriteLine($"Total {args[1]} replays found: {i}");
var json = JsonConvert.SerializeObject(jar, Formatting.Indented,
    new JsonSerializerSettings { NullValueHandling = NullValueHandling.Ignore });
File.WriteAllText($"{directory}\\{args[1]}_replays.json", json);

Console.WriteLine("Press any key to exit...");
Console.Read();