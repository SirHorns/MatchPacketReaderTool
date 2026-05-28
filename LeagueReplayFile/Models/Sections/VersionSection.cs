namespace LeagueReplayFile.Models.Sections;

public class VersionSection : Section, IJsonSection
{
    public string Text { get; internal set; }
    public void SetValues(string json) => Text = json;
}