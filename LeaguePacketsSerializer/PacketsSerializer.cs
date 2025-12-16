using System;
using System.Collections.Generic;
using System.Linq;
using LeaguePackets;
using LeaguePackets.Game;
using LeaguePacketsSerializer.ENet;
using LeaguePacketsSerializer.Enums;
using LeaguePacketsSerializer.Packets;
using LeaguePacketsSerializer.Parsers;
using LeaguePacketsSerializer.Parsers.ChunkParsers;
using LeaguePacketsSerializer.Replication;

namespace LeaguePacketsSerializer;

public static class PacketsSerializer
{
    private static readonly Dictionary<uint, ReplicationType> _replicationTypes = new();
    
    
    public static void ParsePackets(ref Replay replay)
    {
        Console.WriteLine("Processing packets...");
        
        if (replay.Type == ReplayType.ENET)
        {
            foreach (var ePacket in replay.RawPackets)
            {
                ParsePacket(replay, ePacket);
            }
        }
        else
        {
            foreach (var chunk in replay.Chunks)
            {
                foreach (var ePacket in chunk.ENetPackets)
                {
                    
                    ParseChunkPacket(chunk, ePacket);
                }
            }
        }

        replay.ReplayInfo = new ReplayInfo(
            replay.Sections.Count,
            replay.Chunks.Count, 
            replay.SerializedPackets.Count, 
            replay.SoftBadPackets.Count,
            replay.HardBadPackets.Count,
            string.Join(",", replay.SoftBadPackets.Select(x => x.RawID.ToString()).Distinct()),
            string.Join(",", replay.HardBadPackets.Select(x => x.RawID.ToString()).Distinct()));


        switch (replay.Type)
        {
            case ReplayType.SPECTATOR:
                foreach (var chunk in replay.Chunks)
                {
                    replay.SerializedPackets.AddRange(chunk.SerializedPackets);
                    replay.SoftBadPackets.AddRange(chunk.SoftBadPackets);
                    replay.HardBadPackets.AddRange(chunk.HardBadPackets);
                }
                break;
            case ReplayType.NAN:
            case ReplayType.NFO:
            case ReplayType.ENET:
            default:
                return;
        }
        
        replay = null;
        Console.WriteLine("Finished Serializing Replay!");
    }
    
    //
    
    private static void ParseChunkPacket(Chunk chunk, ENetPacket enetPacket)
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
    }
    
    private static void ParsePacket(Replay replay, ENetPacket rPacket)
    {
        if (rPacket.Channel >= 8)
        {
            return;
        }

        int rawId = rPacket.Bytes[0];
        if (rawId == 254)
        {
            rawId = rPacket.Bytes[5] | rPacket.Bytes[6] << 8;
        }

        try
        {
            var basePacket = BasePacket.Create(rPacket.Bytes, (ChannelID)rPacket.Channel);
            object packetToSerialize = basePacket;
            
            if (basePacket is OnReplication replication)
            {
                packetToSerialize = OnReplication(replication);
            }
            else
            {
                SetReplicationType(basePacket);
            }

            var serializedPacket = CreateSerializePacket(rawId, packetToSerialize, rPacket);
            replay.SerializedPackets.Add(serializedPacket);

            if (rPacket.Channel > 0 && basePacket.ExtraBytes.Length > 0)
            {
                var softBad = SoftBad(rawId, rPacket, basePacket);
                replay.SoftBadPackets.Add(softBad);
            }

            if (basePacket is not IGamePacketsList list)
            {
                return;
            }

            var softBads = SoftBadLoop(list, rPacket);
            foreach (var softBad in softBads)
            {
                replay.SoftBadPackets.Add(softBad);
            }
        }
        catch (Exception exception)
        {
            var hardBad = HardBad(rawId, rPacket, exception);
            replay.HardBadPackets.Add(hardBad);
        }
    }
    
    private static void SetReplicationType(BasePacket packet)
    {
        switch (packet)
        {
            case S2C_CreateTurret ct:
                _replicationTypes[ct.NetID] = ReplicationType.Turret;
                break;
            case S2C_CreateHero ch:
                _replicationTypes[ch.NetID] = ReplicationType.Hero;
                break;
            case S2C_CreateNeutral cn:
                _replicationTypes[cn.NetID] = ReplicationType.Monster;
                break;
            case CHAR_SpawnPet sp:
                //TODO: verify
                _replicationTypes[sp.SenderNetID] = ReplicationType.Pet;
                break;
            case SpawnMinionS2C sm:
                _replicationTypes[sm.NetID] = ReplicationType.Minion;
                break;
            case Barrack_SpawnUnit su:
                //TODO: verify
                _replicationTypes[su.SenderNetID] = ReplicationType.LaneMinion;
                break;
            case OnEnterVisibilityClient vp:
                foreach (var subPacket in vp.Packets)
                {
                    SetReplicationType(subPacket);
                }
                break;
            case Batched bp:
                foreach (var subPacket in bp.Packets)
                {
                    SetReplicationType(subPacket);
                }
                break;
        }
    }
    
    private static object OnReplication(OnReplication onReplication)
    {
        var packetToSerialize = new FakeOnReplication
        {
            SyncID = onReplication.SyncID,
            SenderNetID = onReplication.SenderNetID,
            ExtraBytes = onReplication.ExtraBytes
        };

        foreach (var rd in onReplication.ReplicationData)
        {
            var netId = rd.UnitNetID;
            var values = new Replicate[6, 32];
            var replicationType = ReplicationType.Unknown;
            if (netId >= 0xFF000000)
            {
                replicationType = ReplicationType.Building;
            }
            else if (false /*netID >= 0x40000000*/)
            {
                //TODO: investigate
                replicationType = ReplicationType.Turret;
            }
            else if (!_replicationTypes.TryGetValue(netId, out replicationType))
            {
                //Console.WriteLine($"WARNING: The type of NetId: #{netId} is Unknown ");
                continue;
            }

            for (byte primaryId = 0; primaryId < 6; primaryId++)
            {
                uint secondaryIdArray = rd.Data[primaryId].Item1;
                if (secondaryIdArray == 0)
                {
                    continue;
                }

                int i = 0;
                var bytes = rd.Data[primaryId].Item2;

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

    private static SerializedPacket CreateSerializePacket(int rawId, object packetToSerialize, ENetPacket rPacket)
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
            Data = packetToSerialize,
            Time = rPacket.Time,
            RawChannel = rPacket.Channel,
        };
        return pkt;
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
    
    private static void DumpState(byte[] bytes, int i, ReplicationType replicationType, byte primaryId, byte secondaryId)
    {
        var s1 = $"bytes = new byte[{bytes.Length}]{{ {string.Join(", ", bytes)} }}; i = {i}; ";
        var s2 = $"type = {replicationType}; primaryId = {primaryId}; secondaryId = {secondaryId}";
        Console.WriteLine($"{s1}{s2}");
    }
}