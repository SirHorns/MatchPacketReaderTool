using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ReplayNamesUnhasher;

public partial class Unhasher
{
    public void UnhashReplay()
    {
        Console.WriteLine("Unhashing Replay...");
        for (i = 0; i < _replay.Count; i++)
        {
            var packetInfo = _replay[i].SelectToken("Packet").ToArray();

            for (k = 0; k < packetInfo.Count(); k++)
            {
                Console.WriteLine(k);
                ProcessProperty(packetInfo[k] as JProperty);
            }
        };
        Console.WriteLine("Finished Unhasing Replay!");
    }
    
    public Task UnhashReplay(JArray packets, string outputPath)
    {
        Console.WriteLine("Unhashing Replay...");
        
        for (i = 0; i < packets.Count; i++)
        {
            var packetInfo = packets[i].SelectToken("Packet");
            UnhashPacket(packetInfo);
        };
        
        Console.WriteLine("Finished Unhasing Replay!");
        
        using var fileStream = File.CreateText(outputPath.Replace(".lrf", "_Unhashed.json"));
        var jsonSerializer = new JsonSerializer
        {
            Formatting = Formatting.Indented
        };
        jsonSerializer.Serialize(fileStream, packets);
        
        GC.Collect();
        return Task.CompletedTask;
    }

    private void UnhashPacket(JToken jtoken)
    {
        var tokens = jtoken.ToArray();
        for (k = 0; k < tokens.Length; k++)
        {
            var jprop = tokens[k] as JProperty;
            ProcessProperty(jprop);
        }
    }
    
    public string Unhash(string json)
    {
        var token = JToken.Parse(json);
        var packetInfo = token.SelectToken("Data").ToArray();

        for (var index = 0; index < packetInfo.Length; index++)
        {
            var jProp = packetInfo[index] as JProperty;
            ProcessProperty(jProp);
        }

        return token.ToString();
    }
}