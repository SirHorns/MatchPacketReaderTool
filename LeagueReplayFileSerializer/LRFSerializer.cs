using LeaguePackets;
using LeaguePackets.Game;
using LeagueReplayFile;
using LeagueReplayFile.Enums;
using LeagueReplayFile.LRFs;
using LeagueReplayFile.Models.Sections;
using LeagueReplayFile.Protocols.ENet;
using LeagueReplayFileSerializer.Data;

namespace LeagueReplayFileSerializer;

public static partial class LRFSerializer
{

    static LRFSerializer() { }

    /// <summary>
    /// Converted ENetPackets into LeaguePackets then Serializes them
    /// </summary>
    /// <param name="lrf"></param>
    /// <returns>SLRF object containing converted packet data</returns>
    public static SLRF? Serialize(LRF lrf)
    {
        var slrf = new SLRF()
        {
            Type = lrf.Type,
            BasicHeader = lrf.BasicHeader,
            MetaData = lrf.MetaData,
        };
        switch (lrf.Type)
        {
            case LRFType.HTTP:
                var lrfSections = (lrf as HttpLRF).Sections;
                SerializeSections(ref lrfSections);
                slrf.Sections = lrfSections;
                break;
            case LRFType.NFO:
            case LRFType.ENET:
                var packets = SerializePackets((lrf as StreamLRF).Packets);
                slrf.Packets = packets;
                break;
            case LRFType.NAN:
            default:
                Console.WriteLine($"Unable to serialize LRF: {lrf.Type}");
                return null;
        }
        
        return slrf;
    }

    /// <summary>
    /// Convert ENetPackets into LeaguePackets
    /// </summary>
    /// <param name="eNetPackets"></param>
    /// <returns></returns>
    public static List<BasePacket> ConvertENetPackets(List<ENetPacket> eNetPackets)
    {
        Console.WriteLine("Converting ENet Packets");
        List<BasePacket> basePackets = [];
        // gather our League of Legends packets
        var count = 0;
        var unk = 0;
        foreach (var eNetPacket in eNetPackets)
        {
            var leaguePacket = BasePacket.Create(eNetPacket.Bytes, (ChannelID)eNetPacket.Channel);
            if (leaguePacket is UnknownPacket)
            {
                unk++;
                var rawID = GetID(eNetPacket);
                Console.WriteLine($"Packet sent over non-standard channel - ID: {rawID} Channel: {eNetPacket.Channel}");
            }
            else
            {
                count++;
            }
            basePackets.Add(leaguePacket);
        }
        Console.WriteLine($"Normal: {count} Unknown: {unk}");
        return basePackets;
    }

    /// <summary>
    /// Serialize League Packets
    /// </summary>
    /// <param name="eNetPackets"></param>
    /// <param name="basePackets"></param>
    /// <returns></returns>
    public static List<SerializedPacket> SerializeLeaguePackets(List<ENetPacket> eNetPackets, List<BasePacket> basePackets)
    {
        var serializedPackets = new List<SerializedPacket>();
        // ngl idk a better way to do this
        for (var i = 0; i < eNetPackets.Count; i++)
        {
            serializedPackets.Add(new SerializedPacket()
            {
                RawID = 0,
                Type = "N/A",
                ChannelID = ChannelID.Default,
                RawChannel = 0,
                Time = 0,
                Packet = default
            });
        }

        int succeeded = 0;
        int failed = 0;
        // iterate over our set to create new SerializedPackets
        for (var i = 0; i < eNetPackets.Count; i++)
        {
            var eNetPacket = eNetPackets[i];
            var basePacket = basePackets[i];
            var rawId = GetID(eNetPacket);
            var serializedPacket = Parse(eNetPacket, basePacket, rawId);
            if (serializedPacket is null)
            {
                serializedPackets[i].RawID = rawId;
                serializedPackets[i].RawChannel = eNetPacket.Channel;
                serializedPackets[i].Time = eNetPacket.Time;
                failed++;
                continue;
            }

            succeeded++;
            serializedPackets[i] = serializedPacket;
        }
        Console.WriteLine($"Success: {succeeded} Failed: {failed}");
        return serializedPackets;
    }
    
    //
    
    private static List<SerializedPacket> SerializePackets(List<ENetPacket> eNetPackets)
    {
        // gather our League of Legends packets
        var basePackets =  ConvertENetPackets(eNetPackets);
        Console.WriteLine("Serializing League Packets");
        var serializedPackets = SerializeLeaguePackets(eNetPackets, basePackets);
        return serializedPackets;
    }

    private static void SerializeSections(ref List<Section> sections)
    {
        for (var i = 0; i < sections.Count; i++)
        {
            var section = sections[i];
            switch (section)
            {
                case GameDataChunkSection gameDataSection:
                    var sgds = new SerializedGameDataSection()
                    {
                        Type = gameDataSection.Type,
                        Http = gameDataSection.Http,
                        Data = gameDataSection.Data,
                        Time = gameDataSection.Time,
                        
                        ID = gameDataSection.ID,
                        Chunk = new SerializedChunk()
                        {
                            ID = gameDataSection.Chunk.ID,
                            Packets = SerializePackets(gameDataSection.Chunk.Packets)
                        }
                    };
                    sections[i] = sgds;
                    break;
                case KeyFrameSection keyFrameSection:
                    var skfs = new SerializedKeyFrameSection()
                    {
                        Type = keyFrameSection.Type,
                        Http = keyFrameSection.Http,
                        Data = keyFrameSection.Data,
                        Time = keyFrameSection.Time,
                        
                        ID = keyFrameSection.ID,
                        Packets = SerializePackets(keyFrameSection.Packets)
                    };
                    sections[i] = skfs;
                    break;
                default:
                    continue;
            }
        }
    }

    private static int GetID(ENetPacket eNetPacket)
    {
        int rawId = eNetPacket.Bytes[0];
        if (rawId == 254)
        {
            rawId = eNetPacket.Bytes[5] | eNetPacket.Bytes[6] << 8;
        }
        return rawId;
    }
    
    private static SerializedPacket? Parse(ENetPacket eNetPacket, BasePacket basePacket, int rawId)
    {
        SerializedPacket? serializedPacket = new SerializedPacket()
        {
            RawID = rawId,
            RawChannel = eNetPacket.Channel,
            ChannelID = eNetPacket.Channel < 8 ? (ChannelID)eNetPacket.Channel : null,
            Time = eNetPacket.Time
        };
        BadPacket[] softBads = [];
        BadPacket[] hardBads = [];
        try
        {
            object serialized = basePacket;
            serializedPacket.Packet = serialized;
            var type = "";
            var channel = (ChannelID)eNetPacket.Channel;
            switch (channel)
            {
                case ChannelID.Default:
                    type = "KeyCheckPacket";
                    break;
                case ChannelID.ClientToServer:
                case ChannelID.SynchClock:
                case ChannelID.Broadcast:
                case ChannelID.BroadcastUnreliable:
                    type = ((GamePacketID)rawId).ToString();
                    break;
                case ChannelID.Chat:
                case ChannelID.QuickChat:
                case ChannelID.LoadingScreen:
                    type = ((LoadScreenPacketID)rawId).ToString();
                    break;
                default:
                    type = "Unknown";
                    break;
            }

            serializedPacket.Type = type;
            
            if (eNetPacket.Channel > 0 && basePacket.ExtraBytes.Length > 0)
            {
                var softBad = SoftBad(rawId, eNetPacket, basePacket);
            }

            if (basePacket is IGamePacketsList parentPacket && parentPacket.Packets.Count > 0)
            {
                var res  = SoftBadLoop(parentPacket, eNetPacket);
                foreach (var softBad in res)
                {
                }
            }
        }
        catch (Exception exception)
        {
            var hardBad = HardBad(rawId, eNetPacket, exception);
        }

        return serializedPacket;
    }

    private static BadPacket SoftBad(int rawId, ENetPacket rPacket, BasePacket packet)
    {
        var softBad = new BadPacket()
        {
            RawID = rawId,
            Raw = rPacket.Bytes,
            RawChannel = rPacket.Channel,
            Error = $"Extra bytes: {Convert.ToBase64String(packet.ExtraBytes)}"
        };

        return softBad;
    }

    private static BadPacket[] SoftBadLoop(IGamePacketsList list, ENetPacket rPacket)
    {
        var bads = new List<BadPacket>();
        foreach (var packet2 in list.Packets)
        {
            if (rPacket.Channel <= 0 || packet2.ExtraBytes.Length <= 0)
            {
                continue;
            }

            var error = $"Extra bytes in {packet2.GetType().Name}: {Convert.ToBase64String(packet2.ExtraBytes)}";
            var softBad = new BadPacket()
            {
                RawID = (int)packet2.ID,
                Raw = rPacket.Bytes,
                RawChannel = rPacket.Channel,
                Error = error,
            };
            bads.Add(softBad);
        }

        return bads.ToArray();
    }

    private static BadPacket HardBad(int rawId, ENetPacket rPacket, Exception exception)
    {
        var hardBad = new BadPacket()
        {
            RawID = rawId,
            Raw = rPacket.Bytes,
            RawChannel = rPacket.Channel,
            Error = exception.ToString(),
        };
        return hardBad;
    }
}