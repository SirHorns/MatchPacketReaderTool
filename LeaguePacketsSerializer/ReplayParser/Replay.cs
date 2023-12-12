using System.Collections.Generic;
using LeaguePacketsSerializer;
using Newtonsoft.Json;

namespace ENetUnpack.ReplayParser;

public class Replay
{
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
    
    
}