namespace LeagueReplayFile.Protocols.ENet;

[Flags]
public enum ENetCommandFlags : byte
{
    NONE = 0,
    ACKNOWLEDGE = (1 << 7),
    UNSEQUENCED = (1 << 6),
    ACKNOWLEDGE_UNSEQUENCED = ACKNOWLEDGE | UNSEQUENCED,
}