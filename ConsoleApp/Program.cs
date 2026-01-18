// See https://aka.ms/new-console-template for more information

using LeagueReplayFile;
using LeagueReplayFile.Enums;
using LeagueReplayFileSerializer;
using Newtonsoft.Json;
using ReplayNamesUnhasher;

string _serializedDirectory = "Serialized";

Directory.CreateDirectory(_serializedDirectory);

//ReplaySerializer serializer = new();
string path;
if (args.Length == 0)
{
    Console.WriteLine("Provide Path:");
    path = Console.ReadLine();
}
else
{
    path = args[0];
}
var dirs = Directory.EnumerateFiles($"{path}\\", $"*.lrf", SearchOption.AllDirectories);
var count = dirs.Count();
Console.WriteLine($"Total replays found: {count}");
var i = 0;

var unhasher = new Unhasher();

unhasher.Initialize();

foreach (var lrfPath in dirs)
{
    ++i;
    var fileName = $"{Path.GetFileNameWithoutExtension(lrfPath)}.slrf";
    if (File.Exists($"{_serializedDirectory}/{fileName}"))
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
        File.WriteAllText($"{_serializedDirectory}/{fileName}", json);
    }
    catch (Exception e)
    {
        Console.WriteLine($"[{rid}]: Error - {e}");
    }
}

Console.WriteLine("Done");
Console.Read();