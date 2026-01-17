using LeaguePackets;
using LeaguePackets.Game;
using LeaguePacketsSerializer.Packets;
using LeagueReplayFile;
using LeagueReplayFile.Enums;
using LeagueReplayFile.Protocols.ENet;

namespace LeaguePacketsSerializer;

public partial class LRFSerializer
{
    private readonly SortedDictionary<uint, GameObjectTypes> _netIdToTypesMap = [];

    public LRFSerializer() { }

    public SLRF CreateSerializedLRF(LRF lrf)
    {
        var slrf = new SLRF()
        {
            Type = lrf.Type,
            BasicHeader = lrf.BasicHeader,
            MetaData = lrf.MetaData,
        };
        switch (lrf.Type)
        {
            case LRFTypes.NAN:
                break;
            case LRFTypes.NFO:
                break;
            case LRFTypes.SPECTATOR:
                break;
            case LRFTypes.ENET:
                var packets = WIP(lrf.ENetPackets.ToArray());
                slrf.Packets = packets;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return slrf;
    }
    
    //
    
    private List<SerializedPacket>  WIP(IList<ENetPacket> eNetPackets)
    {
        var serializedPackets = new List<SerializedPacket>();
        // ngl idk a better way to do this
        for (int i = 0; i < eNetPackets.Count; i++)
        {
            serializedPackets.Add(null);
        }

        List<BasePacket?> basePackets = [];

        var index = -1;
        var size = eNetPackets.Count - 1;
        // gather our League of Legends packets
        foreach (var eNetPacket in eNetPackets)
        {
            index++;
            if (eNetPacket.Channel >= 8)
            {
                // Sometimes we get packets with unknown channels
                // Their use or what they do is unknown as of now
                basePackets.Add(null);
                serializedPackets[index] = new SerializedPacket()
                {
                    RawID = -1,
                    Type = "Unknown",
                    ChannelID = null,
                    RawChannel = eNetPacket.Channel,
                    Time = eNetPacket.Time,
                    Packet = eNetPacket
                };
                continue;
            }

            try
            {
                var basePacket = BasePacket.Create(eNetPacket.Bytes, (ChannelID)eNetPacket.Channel);
                basePackets.Add(basePacket);
            }
            catch (Exception e)
            {
                basePackets.Add(null);
            }
        }

        // iterate over our set to create new SerializedPackets
        for (var i = 0; i < eNetPackets.Count; i++)
        {
            var eNetPacket = eNetPackets[i];
            var basePacket = basePackets[i];
            if (basePacket is null)
            {
                continue;
            }
            var rawId = GetID(eNetPacket);
            var serializedPacket = Parse(eNetPacket, basePacket, rawId);
            serializedPackets.Add(serializedPacket);
        }

        return serializedPackets;
    }

    private int GetID(ENetPacket eNetPacket)
    {
        int rawId = eNetPacket.Bytes[0];
        if (rawId == 254)
        {
            rawId = eNetPacket.Bytes[5] | eNetPacket.Bytes[6] << 8;
        }
        return rawId;
    }
    
    private SerializedPacket? Parse(ENetPacket eNetPacket, BasePacket basePacket, int rawId)
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
    
    private void RegisterGameObjectType(BasePacket packet)
    {
        uint netID = 0;
        GameObjectTypes type = GameObjectTypes.Unknown;
        switch (packet)
        {
            case S2C_CreateTurret pkt:
                netID = pkt.NetID;
                type = GameObjectTypes.ObjAIBase_Turret;
                break;
            case S2C_SpawnTurret pkt:
                netID = pkt.NetID;
                type = GameObjectTypes.ObjAIBase_Turret;
                break;
            case S2C_CreateHero pkt:
                netID = pkt.NetID;
                type = GameObjectTypes.ObjAIBase_Hero;
                break;
            case S2C_CreateNeutral pkt:
                netID = pkt.NetID;
                type = GameObjectTypes.NeutralMinionCamp;
                break;
            case CHAR_SpawnPet pkt:
                netID = pkt.SenderNetID;
                type = GameObjectTypes.AttackableUnit;
                break;
            case SpawnMinionS2C pkt:
                netID = pkt.NetID;
                type = GameObjectTypes.AttackableUnit;
                break;
            case Barrack_SpawnUnit pkt:
                netID = pkt.SenderNetID;
                type = GameObjectTypes.AttackableUnit;
                break;
            case SpawnBotS2C pkt:
                netID = pkt.NetID;
                type = GameObjectTypes.Unknown; // GameObjectTypes.Bot;
                break;
            case SpawnLevelPropS2C pkt:
                netID = pkt.NetID;
                type = GameObjectTypes.LevelProp;
                break;
            case SpawnMarkerS2C pkt:
                netID = pkt.NetID;
                type = GameObjectTypes.ObjAIBase_Marker;
                break;
            case S2C_ForceCreateMissile pkt:
                return;
                netID = pkt.MissileNetID;
                type = GameObjectTypes.Missile;
                break;
            case IGamePacketsList parent:
                foreach (var subPacket in parent.Packets)
                {
                    RegisterGameObjectType(subPacket);
                }
                break;
            default:
                return;
        }

        if (netID == 0)
        {
            return;
        }
        
        if (!_netIdToTypesMap.TryAdd(netID, type))
        {
            //var saved = _netIdToTypesMap[netID];
            //Console.WriteLine($"Attempt Map Override :: {netID} : {saved} -> {type}");
        }
    }

    private BadPacket SoftBad(int rawId, ENetPacket rPacket, BasePacket packet)
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

    private BadPacket[] SoftBadLoop(IGamePacketsList list, ENetPacket rPacket)
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

    private BadPacket HardBad(int rawId, ENetPacket rPacket, Exception exception)
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