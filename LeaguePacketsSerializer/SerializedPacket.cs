using LeaguePackets;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LeaguePacketsSerializer;

public class SerializedPacket
{
    public int RawID { get; set; }
    public object Packet { get; set; }
    public float Time { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ChannelID? ChannelID { get; set; }

    public byte RawChannel { get; set; }
}