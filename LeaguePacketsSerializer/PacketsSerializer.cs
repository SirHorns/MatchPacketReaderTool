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

public class PacketsSerializer
{
    private readonly Dictionary<uint, ReplicationType> _replicationTypes = new();
    
    private Chunk _currentChunk;
    private Replay _replay;
    
    public void ParsePackets(Replay replay)
    {
        Console.WriteLine("Processing packets...");

        _replay = replay;
        
        if (replay.Type == ReplayType.ENET)
        {
            foreach (var ePacket in replay.RawPackets)
            {
                ParsePacket(ePacket);
            }
        }
        else
        {
            foreach (var chunk in replay.Chunks)
            {
                foreach (var ePacket in chunk.ENetPackets)
                {
                    _currentChunk = chunk;
                    ParsePacket(ePacket);
                }
            }
            _currentChunk = null;
        }

        _replay = null;
        Console.WriteLine("Finished Serializing Replay!");
    }
    
    
    private void ParsePacket(ENetPacket rPacket)
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

            SerializePacket(rawId, packetToSerialize, rPacket);

            if (rPacket.Channel > 0 && basePacket.ExtraBytes.Length > 0)
            {
                SoftBad(rawId, rPacket, basePacket);
            }

            if (basePacket is not IGamePacketsList list)
            {
                return;
            }

            SoftBadLoop(list, rPacket);
        }
        catch (Exception exception)
        {
            HardBad(rawId, rPacket, exception);
        }
    }
    
    private void SetReplicationType(BasePacket packet)
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
    
    private object OnReplication(OnReplication onReplication)
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

    private void SerializePacket(int rawId, object packetToSerialize, ENetPacket rPacket)
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
        if (_replay.Type == ReplayType.ENET)
        {
            _replay.SerializedPackets.Add(pkt);
        }
        else
        {
            _currentChunk.SerializedPackets.Add(pkt);
        }
    }

    private void SoftBad(int rawId, ENetPacket rPacket, BasePacket packet)
    {
        var softBad = new BadPacket()
        {
            RawID = rawId,
            Raw = rPacket.Bytes,
            RawChannel = rPacket.Channel,
            Error = $"Extra bytes: {Convert.ToBase64String(packet.ExtraBytes)}"
        };
        if (_replay.Type == ReplayType.ENET)
        {
            _replay.SoftBadPackets.Add(softBad);
        }
        else
        {
            _currentChunk.SoftBadPackets.Add(softBad);
        }
    }

    private void SoftBadLoop(IGamePacketsList list, ENetPacket rPacket)
    {
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
            if (_replay.Type == ReplayType.ENET)
            {
                _replay.SoftBadPackets.Add(softBad);
            }
            else
            {
                _currentChunk.SoftBadPackets.Add(softBad);
            }
        }
    }

    private void HardBad(int rawId, ENetPacket rPacket, Exception exception)
    {
        var hardBad = new BadPacket()
        {
            RawID = rawId,
            Raw = rPacket.Bytes,
            RawChannel = rPacket.Channel,
            Error = exception.ToString(),
        };
        if (_replay.Type == ReplayType.ENET)
        {
            _replay.HardBadPackets.Add(hardBad);
        }
        else
        {
            _currentChunk.HardBadPackets.Add(hardBad);
        }
    }
    
    private void DumpState(byte[] bytes, int i, ReplicationType replicationType, byte primaryId, byte secondaryId)
    {
        var s1 = $"bytes = new byte[{bytes.Length}]{{ {string.Join(", ", bytes)} }}; i = {i}; ";
        var s2 = $"type = {replicationType}; primaryId = {primaryId}; secondaryId = {secondaryId}";
        Console.WriteLine($"{s1}{s2}");
    }
}