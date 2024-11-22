using System;

namespace LeaguePacketsSerializer.Packets;

[Flags]
public enum ENetPacketFlags
{
    Reliable = (1 << 7),
    Unsequenced = (1 << 6),
    ReliableUnsequenced = Reliable | Unsequenced,
    None = 0,
}