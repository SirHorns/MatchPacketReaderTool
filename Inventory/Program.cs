// See https://aka.ms/new-console-template for more information

using LeagueReplayFile;
using LeagueReplayFileSerializer.Enums;

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
foreach (var lrfPath in lrfs)
{
    var fileName = $"{Path.GetFileName(lrfPath)}";
    var stream = File.OpenRead(lrfPath);
    reader = new LRFReader();
    var metaData = reader.ReadMetaData(stream, out var type);
    reader.Dispose();
    foreach (var player in metaData.Players)
    {
        if (player.Champion.ToLowerInvariant().Equals("quinn"))
        {
            Console.WriteLine($"Found Quinn in {lrfPath}");
            Console.Read();
        }
    }
}

Console.WriteLine("Press any key to exit...");
Console.Read();