using LeaguePackets;
using LeaguePackets.Game;
using LeaguePacketsSerializer.Packets;
using LeaguePacketsSerializer.Replication;
using LeagueReplayFile;
using LeagueReplayFile.Protocols.ENet;

namespace LeaguePacketsSerializer;

public partial class PacketsSerializer
{
    private readonly Dictionary<uint, ReplicationType> _replicationTypes = new();
    private readonly SortedDictionary<uint, GameObjectTypes> _netIdToTypesMap = [];


    public SLRF CreateSerializedLRF(LRF lrf)
    {
        var slrf = new SLRF()
        {
            Packets = ParsePackets(lrf.ENetPackets)
        };
        return slrf;
    }
    
    //

    public List<SerializedPacket>  ParsePackets(IEnumerable<ENetPacket> packets)
    {
        Test(packets.ToList());
        
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
    

    private List<SerializedPacket>  Test(IList<ENetPacket> eNetPackets)
    {
        List<SerializedPacket> serializedPackets = [];

        List<BasePacket?> basePackets = [];
        
        foreach (var eNetPacket in eNetPackets)
        {
            if (eNetPacket.Channel >= 8)
            {
                basePackets.Add(null);
                continue;
            }
            var basePacket = BasePacket.Create(eNetPacket.Bytes, (ChannelID)eNetPacket.Channel);
            basePackets.Add(basePacket);
        }

        foreach (var pkt in basePackets)
        {
            if (pkt is null)
            {
                continue;
            }
            RegisterUnitReplicationType(pkt);
            RegisterObj(pkt);
        }

        List<OnReplication> replications = [];
        
        foreach (var packet in basePackets)
        {
            if (packet is not OnReplication onReplication)
            {
                continue;
            }
            replications.Add(onReplication);
        }

        foreach (var replication in replications)
        {
            var syncId = replication.SyncID;
            Console.WriteLine($"[{syncId}]");
            foreach (var rd in replication.ReplicationData)
            {
                var unitNetID = rd.UnitNetID;
                var data = rd.Data;
                if (_netIdToTypesMap.TryGetValue(unitNetID, out var type))
                {
                    Console.WriteLine($"[{unitNetID} / {type}]");
                }
                else
                {
                    Console.WriteLine($"[{unitNetID} / {GameObjectTypes.Unknown}]");
                }
                
                foreach (var (pid, bytes) in data)
                {
                    if (pid == 0)
                    {
                        continue; // nothing to replicate
                    }
                    Console.WriteLine($"<{pid} , [{string.Join(", ", bytes)}]>");
                    for (int sid = 0; sid < 32; sid++)
                    {
                        if (((pid >> sid) & 1) == 0)
                        {
                            continue; // unknown
                        }
                        Console.WriteLine($"[{pid}::{sid}]");
                    }
                }
            }
        }
        
        for (var i = 0; i < eNetPackets.Count; i++)
        {
            var eNetPacket = eNetPackets[i];
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
                serializedPackets.Add(badChannelPacket);
                continue;
            }
            var rawId = GetID(eNetPacket);
            var basePacket = basePackets[i];
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

    private SerializedPacket? Parse(ENetPacket rPacket, BasePacket basePacket, int rawId)
    {
        try
        {
            object packetToSerialize = basePacket;
            
            if (basePacket is OnReplication replication)
            {
                //packetToSerialize = OnReplication(replication);
            }

            var serializedPacket = SerializedPacket.Create(rawId, rPacket, packetToSerialize);

            if (rPacket.Channel > 0 && basePacket.ExtraBytes.Length > 0)
            {
                var softBad = SoftBad(rawId, rPacket, basePacket);
            }

            if (basePacket is IGamePacketsList parentPacket && parentPacket.Packets.Count > 0)
            {
                var softBads = SoftBadLoop(parentPacket, rPacket);
                foreach (var softBad in softBads)
                {
                }
            }

            return serializedPacket;
        }
        catch (Exception exception)
        {
            var hardBad = HardBad(rawId, rPacket, exception);
        }

        return null;
    }
    
    public enum GameObjectTypes
    {
        Unknown,
        InfoPoint,
        EffectEmitter,
        LevelProp,
        Missile,
        ChainMissile = Missile,
        CircleMissile = Missile,
        LineMissile = Missile,
        NeutralMinionCamp,
        AttackableUnit,
        ObjAIBase = AttackableUnit,
        ObjAIBase_Hero = ObjAIBase,
        ObjAIBase_Turret = ObjAIBase,
        ObjAIBase_Minion = ObjAIBase,
        ObjAIBase_Marker = ObjAIBase,
        ObjAIBase_LevelProp = ObjAIBase,
        ObjAIBase_FollowerObject = ObjAIBase,
        ObjBuilding,
        ObjBuilding_Shop = ObjBuilding,
        ObjBuilding_Levelsizer  = ObjBuilding,
        ObjBuilding_NavPoint = ObjBuilding,
        ObjBuilding_Lake = ObjBuilding,
        ObjBuilding_SpawnPoint = ObjBuilding,
        ObjBuildingBarracks = ObjBuilding,
        ObjBuilding_Animated = ObjBuilding,
        ObjAnimated_Turret = ObjBuilding_Animated,
        ObjAnimated_HQ = ObjBuilding_Animated,
        ObjAnimated_BarracksDampener = ObjBuilding_Animated,
        
    }
    
    private void RegisterObj(BasePacket packet)
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
                    RegisterObj(subPacket);
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
            var saved = _netIdToTypesMap[netID];
            Console.WriteLine($"Attempt Map Override :: {netID} : {saved} -> {type}");
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
    
    private void DumpState(byte[] bytes, int i, ReplicationType replicationType, byte primaryId, byte secondaryId)
    {
        var s1 = $"bytes = new byte[{bytes.Length}]{{ {string.Join(", ", bytes)} }}; i = {i}; ";
        var s2 = $"type = {replicationType}; primaryId = {primaryId}; secondaryId = {secondaryId}";
        Console.WriteLine($"{s1}{s2}");
    }
}