using LeaguePackets;
using LeagueReplayFile;
using LeagueReplayFileSerializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReplayNamesUnhasher;

Console.WriteLine("Hello!");

string directory = "";
if (args.Length  > 0)
{
    directory = args[0];
}
else
{
    Console.WriteLine("Provide a directory that contains replays:");
    directory = Console.ReadLine();
}


var lrfs = Directory.EnumerateFiles($"{directory}\\", $"*.lrf", SearchOption.AllDirectories).ToList();
var count = lrfs.Count;
if (lrfs.Count == 0)
{
    Console.WriteLine($"No .lrf files found in \"{directory}\"");
    Console.WriteLine("Press any key to exit...");
    Console.Read();
    return;
}
        
Console.WriteLine($"Total replays found: {count}");

string character = "";
if (args.Length > 1)
{
    character = args[1];
}
else
{
    var loop = true;
    while (loop)
    {
        Console.WriteLine("What character replay do you want to find?");
        character = Console.ReadLine();
        if (string.IsNullOrEmpty(character))
        {
            break;
        }
        character = character.ToLowerInvariant();
        Console.WriteLine($"Is \"{character}\" correct? [y/n]");
        var input = Console.ReadLine();
        input = input?.ToLowerInvariant();
        switch (input)
        {
            case "y":
            case "yes":
                loop = false;
                break;
            case "n":
            case "no":
                break;
        }
    }
}



if (string.IsNullOrEmpty(character))
{
    Console.WriteLine($"Can't search a blank name. Provide a a player champion name to search for");
    Console.WriteLine("Press any key to exit...");
    Console.Read();
    return;
}

Console.WriteLine("Searching replays...");

var i = 0;
List<string> found = [];

Dictionary<string, IEnumerable<string>> map = [];

foreach (var lrfPath in lrfs)
{
    var fileName = $"{Path.GetFileName(lrfPath)}";
    var stream = File.OpenRead(lrfPath);
    var reader = new LRFReader();
    var metaData = reader.ReadMetaData(stream, out var type);
    reader.Dispose();

    if (metaData is null)
    {
        continue;
    }
    
    var champions = metaData.Players.Select(p => p.Champion);
    foreach (var champion in champions)
    {
        if (champion.ToLowerInvariant().Equals(character))
        {
            i++;
            found.Add(lrfPath);
            map.Add(fileName, champions);
            Console.WriteLine($"[{i}]: {fileName} - [{string.Join(", ", champions)}]");
            break;
        }
    }
}

Console.WriteLine($"Found {i} replays containing {character}");

var Unhasher = new Unhasher();
Unhasher.Initialize();

Directory.CreateDirectory($"{directory}/{character}");
File.WriteAllText($"{directory}/{character}/FileMap.json", JsonConvert.SerializeObject(map, Formatting.Indented)); 

foreach (var path in found)
{
    var fileName = Path.GetFileNameWithoutExtension(path);
    try
    {
        var where = $"{directory}/{character}/{fileName}";

        if (Directory.Exists(where))
        {
            continue;
        }
        
        var stream = File.OpenRead(path);
        var reader = new LRFReader();
        var lrf = reader.Read(stream);
        var slrf = LRFSerializer.Serialize(lrf);
        Unhasher.Unhashie(slrf);

        List<SerializedPacket> prespawn = [];
        int j = 0;
        for (; j < slrf.Packets.Count; j++)
        {
            var sp = slrf.Packets[j];
            if (sp.RawID == (int)GamePacketID.S2C_StartSpawn)
            {
                break;
            }
            prespawn.Add(sp);
        }
        
        List<SerializedPacket> spawn = [];
        for (; j < slrf.Packets.Count; j++)
        {
            var sp = slrf.Packets[j];
            spawn.Add(sp);
            if (sp.RawID == (int)GamePacketID.S2C_EndSpawn)
            {
                j++;
                break;
            }
        }
        
        List<SerializedPacket> postspawn = [];
        for (; j < slrf.Packets.Count; j++)
        {
            var sp = slrf.Packets[j];
            if (sp.RawID == (int)GamePacketID.S2C_StartGame)
            {
                break;
            }
            postspawn.Add(sp);
        }
        
        List<SerializedPacket> game = [];
        for (; j < slrf.Packets.Count; j++)
        {
            var sp = slrf.Packets[j];
            game.Add(sp);
        }
        
        Directory.CreateDirectory(where);
        
        var json = JsonConvert.SerializeObject(prespawn, Formatting.Indented);
        File.WriteAllText($"{where}/PreSpawn.json", json);  
        
        json = JsonConvert.SerializeObject(spawn, Formatting.Indented);
        File.WriteAllText($"{where}/Spawn.json", json);  
        
        json = JsonConvert.SerializeObject(postspawn, Formatting.Indented);
        File.WriteAllText($"{where}/PostSpawn.json", json);  
        
        json = JsonConvert.SerializeObject(game, Formatting.Indented);
        File.WriteAllText($"{where}/Game.json", json);  
        
        json = JsonConvert.SerializeObject(slrf.MetaData, Formatting.Indented);
        File.WriteAllText($"{where}/MetaData.json", json);  
    }
    catch (Exception e)
    {
        Console.WriteLine($"Something went wrong with replay: {fileName}");
        Console.WriteLine(e);
    }
}

Console.WriteLine("Press any key to exit...");
Console.Read();