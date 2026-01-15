using LeaguePackets;
using LeaguePackets.Game;
using LeaguePacketsSerializer.Packets;
using LeaguePacketsSerializer.Replication;
using LeagueReplayFile;
using LeagueReplayFile.Protocols.ENet;

namespace LeaguePacketsSerializer;

public class PacketsSerializer
{
    private readonly Dictionary<uint, ReplicationType> _replicationTypes = new();


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
    
    /*private void ParseChunkPacket(Chunk chunk, ENetPacket enetPacket)
    {
        if (enetPacket.Channel >= 8)
        {
            return;
        }

        int rawId = enetPacket.Bytes[0];
        if (rawId == 254)
        {
            rawId = enetPacket.Bytes[5] | enetPacket.Bytes[6] << 8;
        }

        try
        {
            var basePacket = BasePacket.Create(enetPacket.Bytes, (ChannelID)enetPacket.Channel);
            object packetToSerialize = basePacket;
            
            if (basePacket is OnReplication replication)
            {
                packetToSerialize = OnReplication(replication);
            }
            else
            {
                SetReplicationType(basePacket);
            }

            var serializedPacket = CreateSerializePacket(rawId, packetToSerialize, enetPacket);
            chunk.SerializedPackets.Add(serializedPacket);
            if (enetPacket.Channel > 0 && basePacket.ExtraBytes.Length > 0)
            {
                var softBad = SoftBad(rawId, enetPacket, basePacket);
                chunk.SoftBadPackets.Add(softBad);
            }

            if (basePacket is not IGamePacketsList list)
            {
                return;
            }

            var softBads = SoftBadLoop(list, enetPacket);
            foreach (var softBad in softBads)
            {
                chunk.SoftBadPackets.Add(softBad);
            }
        }
        catch (Exception exception)
        {
            var hardBad = HardBad(rawId, enetPacket, exception);
            chunk.HardBadPackets.Add(hardBad);
        }
    }*/


    private List<SerializedPacket>  Test(IList<ENetPacket> eNetPackets)
    {
        int index = 0;
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

        foreach (var basePacket in basePackets)
        {
            if (basePacket is null)
            {
                continue;
            }
            RegisterUnitReplicationType(basePacket);
        }


        for (int i = 0; i < eNetPackets.Count(); i++)
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
            int rawId = eNetPacket.Bytes[0];
            if (rawId == 254)
            {
                rawId = eNetPacket.Bytes[5] | eNetPacket.Bytes[6] << 8;
            }

            var basePacket = basePackets[i];
            var serializedPacket = Parse(eNetPacket, basePacket, rawId);
            serializedPackets.Add(serializedPacket);
        }

        return serializedPackets;
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
                packetToSerialize = OnReplication(replication);
            }

            var serializedPacket = CreateSerializePacket(rawId, packetToSerialize, rPacket);

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
                _replicationTypes[sb.SenderNetID] = ReplicationType.Bot;
                break;
            case SpawnLevelPropS2C slp:
                _replicationTypes[slp.SenderNetID] = ReplicationType.Prop;
                break;
            case SpawnMarkerS2C sp:
                _replicationTypes[sp.SenderNetID] = ReplicationType.Marker;
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
            var values = new Replicate[6, 32];
            var replicationType = ReplicationType.Unknown;
            
            
            if (_replicationTypes.TryGetValue(netId, out replicationType))
            {
                Console.WriteLine($"Unit NetID: {netId}:{replicationType}");
            }
            else switch (netId)
            {
                case >= 0xFF000000:
                    Console.WriteLine($"Unit NetID: {netId}:{ReplicationType.Building}");
                    replicationType = ReplicationType.Building;
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

                    bool? isFloat = DataDict.IsFloat((int)replicationType, primaryId, secondaryId);
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

                            values[primaryId, secondaryId] = new Replicate(value);
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
                            values[primaryId, secondaryId] = new Replicate(value);
                        }
                        catch (Exception e)
                        {
                            DumpState(bytes, i, replicationType, primaryId, secondaryId);
                        }
                    }
                }
            }

            packetToSerialize.ReplicationData.Add(new FakeReplicationData(netId, DataDict.Gen(replicationType, values)));
        }

        return packetToSerialize;
    }

    private SerializedPacket CreateSerializePacket(int rawId, object packetToSerialize, ENetPacket rPacket)
    {
        var type = "";

        switch ((ChannelID)rPacket.Channel)
        {
            case ChannelID.Default:
                type = "Registry";
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
        
        var pkt = new SerializedPacket
        {
            RawID = rawId,
            Type = type,
            ChannelID = rPacket.Channel < 8 ? (ChannelID)rPacket.Channel : null,
            Packet = packetToSerialize,
            Time = rPacket.Time,
            RawChannel = rPacket.Channel,
        };
        return pkt;
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