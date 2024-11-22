using LeaguePackets;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LeaguePacketsSerializer.Packets;

public class SerializedPacket
{
    public int RawID { get; set; }
    public string Type { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ChannelID? ChannelID { get; set; }
    
    public byte RawChannel { get; set; }

    
    public float Time { get; set; }
    public object Data { get; set; }
}