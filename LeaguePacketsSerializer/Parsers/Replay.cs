using System.Collections.Generic;
using LeaguePacketsSerializer.Packets;
using LeaguePacketsSerializer.Parsers.ChunkParsers;
using Newtonsoft.Json;
using ENetPacket = LeaguePacketsSerializer.Parsers.ChunkParsers.ENetPacket;

namespace LeaguePacketsSerializer.Parsers;

public class Replay
{
    [JsonProperty("BasicHeader")]
    public BasicHeader BasicHeader { get; set; }
    [JsonProperty("MetaData")]
    public MetaData MetaData { get; set; }
    [JsonProperty("SerializedPackets")]
    public List<SerializedPacket> SerializedPackets { get; } = new();
    [JsonProperty("HardBadPackets ")]
    public List<BadPacket> HardBadPackets { get; } = new();
    [JsonProperty("SoftBadPackets")]
    public List<BadPacket> SoftBadPackets { get; } = new();
    
    [JsonProperty("Results")]

    public ReplayResultInfo ReplayResultInfo { get; internal set; }

    [JsonIgnore]
    public List<ENetPacket> RawPackets { get; set; }
    [JsonIgnore]
    public List<Chunk> Chunks { get; set; }
}