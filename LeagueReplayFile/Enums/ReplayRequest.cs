namespace LeagueReplayFile.Enums;

public enum ReplayRequest
{
    NONE,
    VERSION, //text
    GAME_META_DATA, //text
    LAST_CHUNK_INFO, //text
    KEY_FRAME, //data
    GAME_DATA_CHUNK, //data
    END_OF_GAME_STATS //text
}