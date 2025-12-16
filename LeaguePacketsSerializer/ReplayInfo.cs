namespace LeaguePacketsSerializer;

public record ReplayInfo(int Sections, int Chunks, int Good, int Soft, int Hard, string SoftBadIds, string HardBadIds);