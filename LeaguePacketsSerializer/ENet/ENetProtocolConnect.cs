using System.IO;
using LeaguePacketsSerializer.Parsers;

namespace LeaguePacketsSerializer.ENet;

public class ENetProtocolConnect : ENetProtocol
{
    public ushort OutgoingPeerID { get; set; }
    public ushort MTU { get; set; }
    public uint WindowSize { get; set; }
    public uint ChannelCount { get; set; }
    public uint IncomingBandwidth { get; set; }
    public uint OutgoingBandwidth { get; set; }
    public uint PacketThrottleInterval { get; set; }
    public uint PacketThrottleAcceleration { get; set; }
    public uint PacketThrottleDeceleration { get; set; }
    public uint SessionID { get; set; }
    public ENetProtocolConnect(ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader protocolCommandHeader, BinaryReader reader)
    {
        switch(protocolHeader.ENetLeagueVersion)
        {
            case ENetLeagueVersion.Seasson12:
                OutgoingPeerID = reader.ReadUInt16(true);
                break;
            case ENetLeagueVersion.Seasson34:
            case ENetLeagueVersion.Patch420:
                OutgoingPeerID = reader.ReadByte();
                reader.ReadByte();
                break;
        }
        MTU = reader.ReadUInt16(true);
        WindowSize = reader.ReadUInt32(true);
        ChannelCount = reader.ReadUInt32(true);
        IncomingBandwidth = reader.ReadUInt32(true);
        OutgoingBandwidth = reader.ReadUInt32(true);
        PacketThrottleInterval = reader.ReadUInt32(true);
        PacketThrottleAcceleration = reader.ReadUInt32(true);
        PacketThrottleDeceleration = reader.ReadUInt32(true);
        switch (protocolHeader.ENetLeagueVersion)
        {
            case ENetLeagueVersion.Seasson12:
                SessionID = reader.ReadUInt32(true);
                break;
            case ENetLeagueVersion.Seasson34:
            case ENetLeagueVersion.Patch420:
                SessionID = reader.ReadByte();
                reader.ReadExactBytes(3);
                break;
        }

    }
}