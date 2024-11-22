namespace LeaguePacketsSerializer.ReplayParser;

public enum HttpState
{
    Done,
    GetText,
    GetBinary,
    ContinueText,
    ContinueBinary,
}