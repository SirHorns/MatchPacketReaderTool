using System.Collections.Generic;
using LeaguePacketsSerializer.ENet;
using LeaguePacketsSerializer.Enums;
using LeaguePacketsSerializer.Packets;
using LeaguePacketsSerializer.Parsers.ChunkParsers;

namespace LeaguePacketsSerializer.Parsers;

public class Replay
{
    public ReplayType Type { get; internal set; }

    public BasicHeader BasicHeader { get; internal set; }
    public MetaData MetaData { get; internal set; }
    public List<Section> Sections { get; internal set; }
    public List<Chunk> Chunks { get; internal init; }
    public List<ENetPacket> RawPackets { get; internal init; }
    
    public ReplayInfo Info { get; internal set; }


    public List<SerializedPacket> SerializedPackets { get; } = new();
    public List<BadPacket> SoftBadPackets { get; } = new();
    public List<BadPacket> HardBadPackets { get; } = new();

    internal void Update()
    {
        if (Type == ReplayType.ENET)
        {
            return;
        }

        if (Type == ReplayType.SPECTATOR)
        {
            foreach (var chunk in Chunks)
            {
                SerializedPackets.AddRange(chunk.SerializedPackets);
                SoftBadPackets.AddRange(chunk.SoftBadPackets);
                HardBadPackets.AddRange(chunk.HardBadPackets);
            }
        }
    }
}