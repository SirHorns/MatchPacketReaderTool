// See https://aka.ms/new-console-template for more information

using LeaguePacketsSerializer;
using LeagueReplayFile;
using LeagueReplayFile.Enums;


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

List<SLRF> slrfs = [];

var dirs = Directory.EnumerateFiles($"{path}\\", $"*.lrf", SearchOption.AllDirectories);
var count = dirs.Count();
Console.WriteLine($"Total replays found: {count}");
var i = 0;
foreach (var lrfPath in dirs)
{
    ++i;
    var rid = $"{i}/{count}";
    try
    {
        var stream = File.OpenRead(lrfPath);
        var reader = new LRFReader();
        var lrf = reader.Read(stream);
        Console.Write($"[{rid}]: {lrf.Type}");
        var serializer = new PacketsSerializer();
        var slrf = serializer.CreateSerializedLRF(lrf);
        slrfs.Add(slrf);
    }
    catch (Exception e)
    {
        Console.WriteLine($"[{rid}]: Error - {e}");
    }
}

Console.WriteLine("Done");
Console.Read();