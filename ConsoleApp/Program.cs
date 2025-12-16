// See https://aka.ms/new-console-template for more information

using LeaguePacketsSerializer;
using LeagueReplayFile;

ReplaySerializer serializer = new();
Console.WriteLine("Provide Path:");

var path = Console.ReadLine();

var dirs = Directory.EnumerateFiles($"{path}\\", $"*.lrf", SearchOption.AllDirectories);
var i = 0;
foreach (var lrfPath in dirs)
{
    try
    {
        var stream = LRF.Read(lrfPath);
        var replay = serializer.Serialize(stream);
        Console.WriteLine($"[{i}]: Parsed");
    }
    catch (Exception e)
    {
        Console.WriteLine($"[{i}]: Error");
        continue;
    }
}

Console.WriteLine("Done");
Console.Read();