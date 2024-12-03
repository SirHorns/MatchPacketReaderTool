using System;

namespace LeaguePacketsSerializer.ENet;

[Flags]
public enum ENetCommandFlag : byte
{
    NONE = 0,
    ACKNOWLEDGE = (1 << 7),
    UNSEQUENCED = (1 << 6),
    ACKNOWLEDGE_UNSEQUENCED = ACKNOWLEDGE | UNSEQUENCED,
}