using LeaguePackets;
using LeaguePackets.Game;
using LeaguePacketsSerializer.Replication;

namespace LeaguePacketsSerializer;

public partial class LRFSerializer
{
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
    
    private void RegisterUnitReplicationType(BasePacket packet)
    {
        switch (packet)
        {
            case S2C_CreateTurret ct:
                _replicationTypes[ct.NetID] = ReplicationType.Turret;
                break;
            case S2C_SpawnTurret st:
                _replicationTypes[st.NetID] = ReplicationType.Turret;
                break;
            case S2C_CreateHero ch:
                _replicationTypes[ch.NetID] = ReplicationType.Hero;
                break;
            case S2C_CreateNeutral cn:
                _replicationTypes[cn.NetID] = ReplicationType.Minion;
                break;
            case CHAR_SpawnPet sp:
                _replicationTypes[sp.SenderNetID] = ReplicationType.Minion;
                break;
            case SpawnMinionS2C sm:
                _replicationTypes[sm.NetID] = ReplicationType.Minion;
                break;
            case Barrack_SpawnUnit su:
                _replicationTypes[su.SenderNetID] = ReplicationType.Minion;
                break;
            case SpawnBotS2C sb:
                //_replicationTypes[sb.SenderNetID] = ReplicationType.Bot;
                break;
            case SpawnLevelPropS2C slp:
                _replicationTypes[slp.SenderNetID] = ReplicationType.Prop;
                break;
            case SpawnMarkerS2C sp:
                //_replicationTypes[sp.SenderNetID] = ReplicationType.Marker;
                break;
            case IGamePacketsList parent:
                foreach (var subPacket in parent.Packets)
                {
                    RegisterUnitReplicationType(subPacket);
                }
                break;
        }
    }
    
    private object OnReplication(OnReplication onReplication)
    {
        var packetToSerialize = new FakeOnReplication
        {
            SyncID = onReplication.SyncID,
            SenderNetID = onReplication.SenderNetID,
            ExtraBytes = onReplication.ExtraBytes
        };

        foreach (var data in onReplication.ReplicationData)
        {
            var netId = data.UnitNetID;
            var values = new ReplicateHold[6, 32];
            var replicationType = ReplicationType.Unknown;
            
            
            if (_replicationTypes.TryGetValue(netId, out replicationType))
            {
                Console.WriteLine($"Unit NetID: {netId}:{replicationType}");
            }
            else switch (netId)
            {
                case >= 0xFF000000:
                    Console.WriteLine($"Unit NetID: {netId}:{ReplicationType.Unknown}");
                    replicationType = ReplicationType.Unknown;
                    break;
                case >= 0x40000000:
                    break;
                    Console.WriteLine($"Unit NetID: {netId}:{ReplicationType.Turret}");
                    //TODO: investigate
                    replicationType = ReplicationType.Turret;
                    break;
                default:
                    //Console.WriteLine($"WARNING: The type of NetId: #{netId} is Unknown ");
                    continue;
            }

            for (byte primaryId = 0; primaryId < 6; primaryId++)
            {
                uint secondaryIdArray = data.Data[primaryId].Item1;
                if (secondaryIdArray == 0)
                {
                    continue;
                }

                int i = 0;
                var bytes = data.Data[primaryId].Item2;

                for (byte secondaryId = 0; secondaryId < 32; secondaryId++)
                {
                    if (((secondaryIdArray >> secondaryId) & 1) == 0)
                    {
                        continue;
                    }

                    var repT = ReplicationDict.GetReplicationValueType((int)replicationType, primaryId, secondaryId);
                    bool? isFloat = false;
                    switch (repT)
                    {
                        case ReplicationDataType.FLOAT:
                            isFloat = true;
                            break;
                        case ReplicationDataType.UINT:
                            break;
                        case ReplicationDataType.BOOL:
                            break;
                        case ReplicationDataType.UNKNOWN:
                            break;
                        case ReplicationDataType.ACTION_STATE:
                            break;
                        case ReplicationDataType.SPELL_DATA_FLAGS:
                            break;
                        case null:
                            isFloat = null;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException();
                    }
                    
                    
                    
                    if (isFloat == null)
                    {
                        Console.WriteLine(
                            $"Warning: the type for [{replicationType}][{primaryId}, {secondaryId}] is unknown");
                        DumpState(bytes, i, replicationType, primaryId, secondaryId);
                        break;
                    }
                    else if (isFloat == true)
                    {
                        try
                        {
                            float value = 0;
                            if (bytes[i] == 0xFF)
                            {
                                i++;
                            }
                            else
                            {
                                int startIndex = i;
                                if (bytes[i] == 0xFE)
                                {
                                    startIndex++;
                                }

                                value = BitConverter.ToSingle(
                                    bytes,
                                    startIndex
                                );
                                i = startIndex + 4;
                            }

                            values[primaryId, secondaryId] = new ReplicateHold(value);
                        }
                        catch (Exception e)
                        {
                            DumpState(bytes, i, replicationType, primaryId, secondaryId);
                        }
                    }
                    else
                    {
                        try
                        {
                            uint value = 0;
                            int j = 0;
                            for (; (bytes[i] & 0x80) != 0; i++, j += 7)
                            {
                                value |= ((uint)bytes[i] & 0x7f) << j;
                            }

                            value |= (uint)bytes[i] << j;
                            i++;
                            values[primaryId, secondaryId] = new ReplicateHold(value);
                        }
                        catch (Exception e)
                        {
                            DumpState(bytes, i, replicationType, primaryId, secondaryId);
                        }
                    }
                }
            }

            packetToSerialize.ReplicationData.Add(new FakeReplicationData(netId, ReplicationDict.LoadMaps(replicationType, values)));
        }

        return packetToSerialize;
    }
}