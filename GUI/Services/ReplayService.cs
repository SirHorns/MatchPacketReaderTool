using System.Linq;
using LeaguePacketsSerializer.Objects;
using LeaguePacketsSerializer.Parsers;

namespace GUI.Services;

public static class ReplayService
{
    private static Replay _replay;

    private static int chunkIndex = 0;
    
    internal static void SetReplay(Replay replay)
    {
        _replay = replay;
    }

    public static string GetVersion() => "1.82.70";//_replay.MetaData.ReplayVersion;

    public static string GetGameMetaData()
    {
        var metaChunk = (MetaDataSection)_replay.Sections.Find(c => c is MetaDataSection);
        return metaChunk.Json;
    }

    public static string GetLastChunkInfo(string platformId, string matchId, string unknown1)
    {
        var sections = _replay.Sections.FindAll(c => c is LastChunkInfoSection);
        var section = sections[chunkIndex];
        chunkIndex++;
        return ((LastChunkInfoSection)section).Json;
    }
}