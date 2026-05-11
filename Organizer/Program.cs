// See https://aka.ms/new-console-template for more information

using LeagueReplayFile;
using LeagueReplayFileSerializer.Enums;

Console.WriteLine("Hello!");
Console.WriteLine("Provide a directory that contains replays:");
var directory = Console.ReadLine();
Console.WriteLine("Confirm this directory as correct: [y/n]:\n" + directory);
while (true)
{
    var input = Console.ReadLine();
    if (input == "y")
    {
        break;
    }
    throw new Exception();
}
Console.WriteLine("Now provide a directory to move replays to:");
var directory2 = Console.ReadLine();
Console.WriteLine("Confirm this directory as correct: [y/n]:\n" + directory2);
while (true)
{
    var input = Console.ReadLine();
    if (input == "y")
    {
        break;
    }
    throw new Exception();
}

var root = $"{directory2}/Organized";

foreach (var type in Enum.GetValues<ReplayType>())
{
    Directory.CreateDirectory($"{root}/{type}");
}

var lrfs = Directory.EnumerateFiles($"{directory}\\", $"*.lrf", SearchOption.AllDirectories).ToList();
var count = lrfs.Count;
if (lrfs.Count == 0)
{
    throw new Exception($"No .lrf files found in \"{directory}\"");
}
        
Console.WriteLine($"Total replays found: {count}");

foreach (var lrfPath in lrfs)
{
    var fileName = $"{Path.GetFileName(lrfPath)}";
    var stream = File.OpenRead(lrfPath);
    var reader = new LRFReader();
    var metaData = reader.ReadMetaData(stream, out var type);
    reader.Dispose();
    Directory.CreateDirectory($"{root}/{type}/{metaData.ClientVersion}");
    var newLocation = $"{root}/{type}/{metaData.ClientVersion}/{fileName}";
    File.Move(lrfPath, newLocation);
    Console.WriteLine($"Moved {fileName} -> {newLocation}");
}

Console.WriteLine($"Replays have been moved to {root}");

Console.WriteLine("Press any key to exit...");
Console.Read();