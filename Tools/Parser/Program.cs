using LeaguePackets;
using LeaguePackets.Game;
using LeaguePackets.LoadScreen;
using LeagueReplayFile;
using LeagueReplayFile.Enums;
using LeagueReplayFile.LRFs;
using LeagueReplayFileSerializer;
using LeagueReplayFileSerializer.Data;
using LeagueReplayFileSerializer.Enums;
using Newtonsoft.Json;
using ReplayNamesUnhasher;

namespace Parser;

public static class Program
{
    private static readonly string SerializedDirectory;
    private static readonly Unhasher Unhasher;

    static Program()
    {
        SerializedDirectory = "Serialized";
        Unhasher = new Unhasher();
        Directory.CreateDirectory(SerializedDirectory);
        foreach (var type in Enum.GetValues<ReplayType>())
        {
            Directory.CreateDirectory($"{SerializedDirectory}/{type}");
        }
    }
    
    static void PrintBreak() => Console.WriteLine("<•······················•<>•······················•>");
    
    public static void Main(string[] args)
    {
        Unhasher.Initialize();
        PrintBreak();
        string path;
        if (args.Length == 0)
        {
            Console.WriteLine("Provide Path to lrf(s):");
            path = Console.ReadLine() ?? "";
        }
        else
        {
            path = args[0];
        }
        
        if(path.EndsWith(".lrf"))
        {
            ReadLRF(path);
        }
        else
        {
            var lrfPaths = GetFilePaths(path);
            if (lrfPaths.Count == 1)
            {
                ReadLRF(lrfPaths[0]);
            }
            else
            {
                ParseLRFs(lrfPaths);
            }
        }

        Console.WriteLine("Done");
        Exit();
    }
    
    private static void Exit() 
    { 
        Console.Write("Press any key to exit..."); 
        while (true)
        { 
            Console.Read(); 
            break;
        }
    }
    
    //⬖•······················•⯁•······················•⬗
    
    private static void ReadLRF(string path)
    {
        var fileName = Path.GetFileNameWithoutExtension(path);
        if (File.Exists($"{SerializedDirectory}/{fileName}"))
        {
            Console.WriteLine($"Skipping {fileName}; Already exists.");
            return;
        }
        Console.WriteLine($"Reading replay...");
        LRF? lrf;
        try
        {
            var stream = File.OpenRead(path);
            lrf = LRFReader.Read(stream);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
        PrintBreak();
        Console.WriteLine($"Spectator: {lrf.MetaData.SpectatorMode}");
        Console.WriteLine($"Platform: {lrf.MetaData.Region}");
        PrintBreak();
        SerializeReplay(lrf, path, fileName);
    }

    private static void SerializeReplay(LRF lrf, string lrfPath, string fileName)
    {
        Console.WriteLine("Serialize replay? (y/n)");
        var answer = Console.ReadLine();
        if (string.IsNullOrEmpty(answer) || !answer.Equals("y", StringComparison.InvariantCultureIgnoreCase))
        {
            return;
        }
        Console.WriteLine($"Serializing replay...");
        SLRF? slrf = null;
        try
        {
            slrf = LRFSerializer.Serialize(lrf);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        } 
        if (slrf == null)
        {
            Console.Error.WriteLine("Couldn't serialize replay. Possible error in stream or no packets to serialize?");
            return;
        }
        Console.WriteLine("Unhash serialized replay? (y/n)");
        answer = Console.ReadLine();
        if (!string.IsNullOrEmpty(answer) && answer.Equals("y", StringComparison.InvariantCultureIgnoreCase))
        {
            Console.WriteLine("Unhashing replay...");
            try
            {
                Unhasher.Unhashie(slrf);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
        Console.WriteLine("Write serialized replay to file? (y/n)");
        answer = Console.ReadLine();
        if (!string.IsNullOrEmpty(answer) && answer.Equals("y", StringComparison.InvariantCultureIgnoreCase))
        {
            WriteToFile(slrf, fileName);
        }
    }
    
    //⬖•······················•⯁•······················•⬗
    
    private static void ParseLRFs(List<string> paths)
    {
        Console.WriteLine("Parsing Packets");
        var i = 0;
        var count = paths.Count;
        List<LRF> lrfs = [];
        foreach (var path in paths)
        {
            Console.WriteLine("<•······················•<>•······················•>");
            ++i; 
            var fileName = $"{Path.GetFileNameWithoutExtension(path)}.slrf";
            if (File.Exists($"{SerializedDirectory}/{fileName}"))
            {
                Console.WriteLine($"Skipping {fileName}; Already exists.");
                continue;
            }
            LRF? lrf = null;
            try
            {
                var stream = File.OpenRead(path);
                lrf = LRFReader.Read(stream);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            var rid = $"{i}/{count}";
            Console.WriteLine($"[{rid}]: {lrf.Type}");
            lrfs.Add(lrf);
        }
        var slrfs = SerializeLRFs(lrfs);
        UnhashSLRFs(slrfs);
    }
    
    private static List<SLRF> SerializeLRFs(List<LRF> lrfs)
    {
        Console.WriteLine("Serialising Replays");
        List<SLRF> slrfs = [];
        foreach (var lrf in lrfs)
        {
            SLRF? slrf = null;
            try
            {
                slrf = LRFSerializer.Serialize(lrf);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
            slrfs.Add(slrf);
        }
        return slrfs;
    }
    
    private static void UnhashSLRFs(List<SLRF> slrfs)
    {
        Console.WriteLine("Unhashing Replays");
        foreach (var slrf in slrfs)
        {
            try
            {
                Unhasher.Unhashie(slrf);
                Unhasher.Reset();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }
        }
    }
    
    //⬖•······················•⯁•······················•⬗
    
    private static void WriteToFile(SLRF slrf, string fileName)
    {
        var path = $"{SerializedDirectory}/{slrf.Type}/{fileName}";
        Directory.CreateDirectory(path);
        
        Console.WriteLine($"Outputted SLRF to {path}");
        
        //
        var metaData = JsonConvert.SerializeObject(slrf.MetaData, Formatting.Indented);
        File.WriteAllText($"{path}/MetaData.json", metaData);  
        //
        var toWrite = new List<SerializedPacket>();
        var index = 0;
        for (; index < slrf.Packets.Count; index++)
        {
            var packet = slrf.Packets[index];
            if (packet.RawID == (int)GamePacketID.S2C_StartSpawn)
            {
                break;
            }
            toWrite.Add(packet);
        }
        
        File.WriteAllText($"{path}/PreSpawn.json", JsonConvert.SerializeObject(toWrite, Formatting.Indented));  
        toWrite.Clear();
        
        for (; index < slrf.Packets.Count; index++)
        {
            var packet = slrf.Packets[index];
            toWrite.Add(packet);
            if (packet.RawID == (int)GamePacketID.S2C_EndSpawn)
            {
                index++; // increment to skip for Game.json
                break;
            }
        }
        
        File.WriteAllText($"{path}/Spawn.json", JsonConvert.SerializeObject(toWrite, Formatting.Indented)); 
        toWrite.Clear();
        
        for (; index < slrf.Packets.Count; index++)
        {
            var packet = slrf.Packets[index];
            toWrite.Add(packet);
        }
        
        File.WriteAllText($"{path}/Game.json", JsonConvert.SerializeObject(toWrite, Formatting.Indented)); 
        toWrite.Clear();
        //
        var json = JsonConvert.SerializeObject(slrf, Formatting.Indented);
        File.WriteAllText($"{path}/{fileName}.slrf", json);   
    }

    private static List<string>? GetFilePaths(string path)
    {
        var lrfs = Directory.EnumerateFiles($"{path}\\", $"*.lrf", SearchOption.AllDirectories).ToList();
        var count = lrfs.Count;
        if (lrfs.Count == 0)
        {
            Console.WriteLine($"No .lrf files found in \"{path}\"");
            Exit();
            return null;
        }
        
        Console.WriteLine($"Total replays found: {count}");
        return lrfs;
    }
    
    private static void PrintPacketCounts(SLRF slrf)
    {
        var dict = new Dictionary<Type, int>();
        switch (slrf.Type)
        {
            case LRFType.NFO:
            case LRFType.ENET:
                foreach (var sp in slrf.Packets)
                {
                    if (sp.Packet is not BasePacket packet)
                    {
                        continue;
                    }

                    var type = packet.GetType();
                    if (dict.TryGetValue(type, out var value))
                    {
                        dict[type] = ++value;
                    }
                    else
                    {
                        dict.Add(type, 1);
                    }
                }
                break;
            case LRFType.HTTP:
                foreach (var section in slrf.Sections)
                {
                    switch (section)
                    {
                        case SerializedGameDataSection serializedGameDataSection:
                            foreach (var sp in serializedGameDataSection.Chunk.Packets)
                            {
                                if (sp.Packet is not BasePacket packet)
                                {
                                    continue;
                                }

                                var type = packet.GetType();
                                if (dict.TryGetValue(type, out var value))
                                {
                                    dict[type] = ++value;
                                }
                                else
                                {
                                    dict.Add(type, 1);
                                }
                            }
                            break;
                        case SerializedKeyFrameSection serializedKeyFrameSection:
                            foreach (var sp in serializedKeyFrameSection.Packets)
                            {
                                if (sp.Packet is not BasePacket packet)
                                {
                                    continue;
                                }

                                var type = packet.GetType();
                                if (dict.TryGetValue(type, out var value))
                                {
                                    dict[type] = ++value;
                                }
                                else
                                {
                                    dict.Add(type, 1);
                                }
                            }
                            break;
                    }
                }
                break;
        }

        foreach (var (type, count) in dict)
        {
            Console.WriteLine("[{0}]:  {1}", type.Name, count );
        }
    }

    private static void PrintMessages(SLRF slrf)
    {
        foreach (var serializedPacket in slrf.Packets)
        {
            if (serializedPacket.Packet is not BasePacket packet)
            {
                continue;
            }

            string msg = "";
            switch (packet)
            {
                case Chat chat:
                    msg = $"[{chat.ChatType}]: <{chat.ClientID}/{chat.NetID}> {chat.Message}";
                    break;
                case QuickChat quickChat:
                    msg = $"<{quickChat.ClientID}> {quickChat.MessageId}";
                    break;
                case S2C_SystemMessage system:
                    msg = $"<{system.SourceNetID}> {system.Message}";
                    break;
                default:
                    continue;
            }
            Console.WriteLine("{0} - [{1}]: {2}", serializedPacket.Time, packet.GetType().Name, msg);
        }
    }
    
}