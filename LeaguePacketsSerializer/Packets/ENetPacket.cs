namespace LeaguePacketsSerializer.Packets;

public class ENetPacket
{
    public float Time { get; set; }
    public byte[] Bytes { get; set; }
    public byte Channel { get; set; }
    public ENetPacketFlags Flags { get; set; }
}