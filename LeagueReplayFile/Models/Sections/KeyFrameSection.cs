using LeagueReplayFile.Protocols.ENet;

namespace LeagueReplayFile.Models.Sections;

public class KeyFrameSection : Section, IStreamSection
{
    public int ID { get; internal set; }
    public List<ENetPacket> Packets { get; set; } = [];
    public byte[] PacketData { get; internal set; }

    public void Write(BinaryWriter writer)
    {
        foreach (var packet in Packets)
        {
            ENetPacket.Write(packet, writer);
        }
    }
}