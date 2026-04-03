namespace LeagueReplayFile.Protocols.ENet;

[Flags]
public enum ENetPacketFlags
{
    None = 0,
    Reliable,
    Unsequenced,
    NoAllocate,
    Sent
}