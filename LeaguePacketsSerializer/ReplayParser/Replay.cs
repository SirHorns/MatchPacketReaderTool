using System.Collections.Generic;
using LeaguePacketsSerializer.Packets;
using Newtonsoft.Json;

namespace LeaguePacketsSerializer.ReplayParser;

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
    
    
    [JsonIgnore]
    public List<ENetPacket> RawPackets { get; set; }
    [JsonIgnore]
    public List<Chunk> Chunks { get; set; }
}