// See https://aka.ms/new-console-template for more information

using System.Text.Json.Serialization;
using Inventory;
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

switch (args[1])
{
    case "-c":
        Searches.FindChampionReplays(directory, lrfs,args[2]);
        break;
    case "-s":
        Searches.FindSpectatorReplays(directory, lrfs);
        break;
    default:
        break;
}

Console.WriteLine("Press any key to exit...");
Console.Read();