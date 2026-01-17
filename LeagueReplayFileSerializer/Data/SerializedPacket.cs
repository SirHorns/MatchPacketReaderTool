using LeaguePackets;
using LeagueReplayFile.Protocols.ENet;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace LeagueReplayFileSerializer.Data;

public class SerializedPacket
{
    public int RawID { get; set; }
    public string Type { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ChannelID? ChannelID { get; set; }
    
    public byte RawChannel { get; set; }

    
    public float Time { get; set; }
    public object Packet { get; set; }

    public static SerializedPacket Create(int rawId, ENetPacket rPacket, object packetToSerialize)
    {
        var type = "";
        var channel = (ChannelID)rPacket.Channel;
        switch (channel)
        {
            case LeaguePackets.ChannelID.Default:
                type = "KeyCheckPacket";
                break;
            case LeaguePackets.ChannelID.ClientToServer:
            case LeaguePackets.ChannelID.SynchClock:
            case LeaguePackets.ChannelID.Broadcast:
            case LeaguePackets.ChannelID.BroadcastUnreliable:
                type = ((GamePacketID)rawId).ToString();
                break;
            case LeaguePackets.ChannelID.Chat:
            case LeaguePackets.ChannelID.QuickChat:
            case LeaguePackets.ChannelID.LoadingScreen:
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
}