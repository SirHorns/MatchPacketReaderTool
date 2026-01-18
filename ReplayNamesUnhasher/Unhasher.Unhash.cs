
using LeaguePackets;
using LeaguePackets.Game;
using LeagueReplayFile.Enums;
using LeagueReplayFileSerializer;
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
            var packetInfo = packets[i].SelectToken("Packet").ToArray();

            for (k = 0; k < packetInfo.Count(); k++)
            {
                ProcessProperty(packetInfo[k] as JProperty);
            }
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


    public void Unhashie(SLRF slrf)
    {
        switch (slrf.Type)
        {
            case LRFTypes.SPECTATOR:
                break;
            case LRFTypes.NFO:
            case LRFTypes.ENET:
                UnhashPackets(slrf.Packets);
                break;
            case LRFTypes.NAN:
            default:
                throw new ArgumentOutOfRangeException();
        }
    }

    private void UnhashPackets(List<SerializedPacket> packets)
    {
        foreach (var sp in packets)
        {
            object result = null;
            if (sp.Packet is not BasePacket packet)
            {
                continue;
            }
            // castinfo
            // talent
            // color
            // argsbuff, argsheal, argsDamage, argsforclient, argsminionkill
            switch (packet)
            {
                case NPC_BuffRemoveGroup:
                    break;
                case C2S_PlayVOCommand:
                    break;
                case NPC_BuffAddGroup:
                    break;
                case S2C_SetSpellData:
                    break;
                case NPC_BuffRemove2:
                    break;
                case NPC_BuffAdd2:
                    break;
                case S2C_PlayContextualEmote:
                    break;
                case S2C_NeutralMinionTimerUpdate:
                    break;
                case S2C_NotifyContextualSituation:
                    break;
                case FX_Create_Group:
                    break;
                case OnReplication onReplication:
                    result = UnhashOnReplication(onReplication);
                    break;
                default:
                    continue;
            }

            if (result is null)
            {
                continue;
            }
            sp.Packet = result;
        }
    }
}