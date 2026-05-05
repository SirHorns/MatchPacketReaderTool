using LeagueReplayFile.Protocols.ENet;

namespace LeagueReplayFile.Models;

public class GameDataChunk
{
    public int ID { get; set; }
    public List<ENetPacket> Packets { get; set; } = [];
    
    public void Write(BinaryWriter writer)
    {
        foreach (var packet in Packets)
        {
            ENetPacket.Write(packet, writer);
        }
    }
}