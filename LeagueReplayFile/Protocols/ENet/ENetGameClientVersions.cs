namespace LeagueReplayFile.Protocols.ENet;

//TODO: figure out what exact versions are to brake at
public enum ENetGameClientVersions
{
    Unknown = 0x00,
    Patch1 = 0x01,
    Patch2 = 0x02,
    Patch3 = 0x03,
    Patch4 = 0x04,
    Patch5 = 0x05,
    Patch6 = 0x06,
    Patch7 = 0x07,
    Patch8 = 0x08,
    Seasson12 = 0x12, // no clue
    Seasson34 = 0x34 // no clue
}
