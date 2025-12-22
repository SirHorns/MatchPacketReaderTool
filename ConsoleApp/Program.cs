// See https://aka.ms/new-console-template for more information

using LeaguePacketsSerializer;
using LeagueReplayFile;
using LeagueReplayFile.Enums;


ReplaySerializer serializer = new();
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
var i = -1;
foreach (var lrfPath in dirs)
{
    ++i;
    try
    {
        var stream = File.OpenRead(lrfPath);
        //var replay = serializer.Serialize(lrf);
        
        var reader = new LRFReader();
        Console.Write($"[{i}]: ");
        var lrf = reader.Read(stream);

        switch (lrf.Type)
        {
            case LRFTypes.NAN:
                break;
            case LRFTypes.NFO:
                break;
            case LRFTypes.SPECTATOR:
                break;
            case LRFTypes.ENET:
                break;
        }
    }
    catch (Exception e)
    {
        Console.WriteLine($"[{i}]: Error - {e}");
    }
}

Console.WriteLine("Done");
Console.Read();