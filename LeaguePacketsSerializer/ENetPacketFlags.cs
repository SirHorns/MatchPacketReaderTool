using System;

namespace LeaguePacketsSerializer;

[Flags]
public enum ENetPacketFlags
{
    Reliable = (1 << 7),
    Unsequenced = (1 << 6),
    ReliableUnsequenced = Reliable | Unsequenced,
    None = 0,
}