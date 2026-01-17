using LeaguePackets;
using LeaguePackets.Game;
using LeaguePacketsSerializer.Packets;
using LeaguePacketsSerializer.Replication;
using LeagueReplayFile;
using LeagueReplayFile.Enums;
using LeagueReplayFile.Protocols.ENet;
using LeagueReplayFileSerializer.Replications;
using Newtonsoft.Json;

namespace LeaguePacketsSerializer;

public partial class LRFSerializer
{
    private readonly Dictionary<uint, ReplicationType> _replicationTypes = new();
    private readonly SortedDictionary<uint, GameObjectTypes> _netIdToTypesMap = [];
    private ReplicationDict ReplicationDict;

    public LRFSerializer()
    {
        ReplicationDict = new ReplicationDict();
    }

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
                var packets = ParsePackets(lrf.ENetPackets);
                slrf.Packets = packets;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }
        
        return slrf;
    }
    
    //

    public List<SerializedPacket>  ParsePackets(IEnumerable<ENetPacket> packets)
    {
        WIP(packets.ToList());
        
        List<SerializedPacket> serialized = [];
        foreach (var packet in packets)
        {
            var res = ParsePacket(packet);
            if (res is null)
            {
                continue;
            }
            serialized.Add(res);
        }

        return serialized;
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
    
    private SerializedPacket? ParsePacket(ENetPacket eNetPacket)
    {
        if (eNetPacket.Channel >= 8)
        {
            var badChannelPacket = new SerializedPacket()
            {
                RawID = -1,
                Type = "Unknown",
                ChannelID = null,
                RawChannel = eNetPacket.Channel,
                Time = eNetPacket.Time,
                Packet = eNetPacket
            };
            return badChannelPacket;
        }

        int rawId = eNetPacket.Bytes[0];
        if (rawId == 254)
        {
            rawId = eNetPacket.Bytes[5] | eNetPacket.Bytes[6] << 8;
        }

        var basePacket = BasePacket.Create(eNetPacket.Bytes, (ChannelID)eNetPacket.Channel);
        RegisterUnitReplicationType(basePacket);
        var serializedPacket = Parse(eNetPacket, basePacket, rawId);
        return serializedPacket;
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
    

    private object SerializeOnReplication(OnReplication replication)
    {
            var syncId = replication.SyncID;
            //Console.WriteLine($"[{syncId}]");
            foreach (var rd in replication.ReplicationData)
            {
                var unitNetID = rd.UnitNetID;
                var data = rd.Data;

                var objectType = _netIdToTypesMap.GetValueOrDefault(unitNetID, GameObjectTypes.Unknown);
                var replicationType = _replicationTypes.GetValueOrDefault(unitNetID, ReplicationType.Unknown );
                //Console.WriteLine($"[ID: {unitNetID} / Obj: {GameObjectTypes.Unknown} / Repl: {replicationType}]");

                Replicant? owner = null;
                
                switch (replicationType)
                {
                    case ReplicationType.Unknown:
                    case ReplicationType.Barracks:
                    case ReplicationType.BarracksDampener:
                        continue;
                    case ReplicationType.Prop:
                        break;
                    case ReplicationType.Hero:
                        break;
                    case ReplicationType.HQ:
                        owner = new HQ();
                        break;
                    case ReplicationType.Minion:
                        owner = new Minion();
                        break;
                    case ReplicationType.Turret:
                        break;
                    default:
                        break;
                }
                for (byte pid = 0; pid < 6; pid++)
                {
                    var unknown = data[pid].Item1;
                    if (unknown == 0)
                    {
                        continue;
                    }

                    var index = 0;
                    var bytes = data[pid].Item2;

                    for (byte sid = 0; sid < 32; sid++)
                    {
                        if (((unknown >> sid) & 1) == 0)
                        {
                            continue; // not sure what the large unknown Tuple uints mean
                        }
                        
                        var replicationDataType = ReplicationDict.GetReplicationValueType((int)replicationType, pid, sid);
                        object? val = null; 
                        try
                        {
                            val = ReplicationDict.GetValue(replicationDataType, bytes, ref index);
                        }
                        catch (Exception e)
                        {
                            //Console.WriteLine($"Failed to find replication {replicationDataType} for a {replicationType}");
                            DumpState(bytes, index, replicationType, pid, sid, e);
                            break;
                        }

                        if (owner is not null)
                        {
                            owner.SetValue(pid, sid, val);
                        }
                    }
                }

                if (owner is null)
                {
                    continue;
                }

                var json = JsonConvert.SerializeObject(owner, Formatting.Indented);
                Console.WriteLine(json);
            }

            return null;
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
    
    private void DumpState(byte[] bytes, int i, ReplicationType replicationType, byte primaryId, byte secondaryId, Exception? e = null)
    {
        
        
        var s1 = $"[{replicationType}]: Index: {primaryId}; SID: {secondaryId};";
        var s2 = $"Bytes: byte[{bytes.Length}]; ReadPos: [{i}]; {{ {string.Join(", ", bytes)} }}; ";
        Console.WriteLine($"{s1}");
        Console.WriteLine($"{s2}");
        if (e is not null)
        {
            Console.WriteLine(e);
        }
    }
}