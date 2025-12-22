using System.Collections.Generic;
using LeaguePacketsSerializer.Enums;
using LeaguePacketsSerializer.Packets;

namespace LeaguePacketsSerializer;

public class Replay
{
    public ReplayType Type { get; internal set; }
    public ReplayInfo ReplayInfo { get; internal set; }
    public List<SerializedPacket> SerializedPackets { get; } = new();
    public List<BadPacket> SoftBadPackets { get; } = new();
    public List<BadPacket> HardBadPackets { get; } = new();
}