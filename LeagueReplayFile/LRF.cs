namespace LeagueReplayFile;

public static class LRF
{
    public static Stream Read(string lrfPath)
    {
        if (!lrfPath.EndsWith(".lrf"))
        {
            throw new FileNotFoundException("No .lrf file was provided");
        }

        return File.OpenRead(lrfPath);
    }
}