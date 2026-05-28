using System.Text.RegularExpressions;
using LeagueReplayFile.Enums;
using LeagueReplayFile.Models;
using LeagueReplayFile.Models.Sections;

namespace LeagueReplayFile.Protocols;

public abstract class HttpProtocol
{
    protected static Regex RE_CONTENT_LEN = new("Content-Length: ([0-9]+)", RegexOptions.IgnoreCase);

    protected static byte[] HTTP_END = { 0x0D, 0x0A, 0x0D, 0x0A };



    public HttpState CurrentHttpState { get; private set; }
    protected List<byte> ByteBuffer { get; }
    protected long ExpectedLengthBuffer { get; set; }
    protected Section? SectionBuffer { get; set; }


    protected HttpProtocol()
    {
        ByteBuffer = [];
        CurrentHttpState = HttpState.Done;
    }


    protected void ParseSegment(DataSegment segment)
    {
        var data = segment.Data;
        var time = segment.Time;
        Console.WriteLine($"[TIME]: {time}");
        switch (CurrentHttpState)
        {
            case HttpState.GetBinary:
                OnGetBinary(data);
                SetHttpState(HttpState.Done);
                break;
            case HttpState.GetText:
                OnGetText(data);
                SetHttpState(HttpState.Done);
                break;
            case HttpState.Done:
                OnDone(data, time, segment);
                break;
            case HttpState.ContinueBinary:
                OnContinueBinary(data);
                break;
            case HttpState.ContinueText:
                OnContinueText(data);
                break;
            default:
                Console.WriteLine($"Skipped Segment: {CurrentHttpState}");
                break;
        }
        Console.WriteLine($"[SECTION]: {SectionBuffer?.GetType().Name ?? "DATA"}");
    }
    
    //<•······················•<>•······················•>
    
    protected abstract void OnGetBinary(byte[] data);
    protected abstract void OnGetText(byte[] data);
    protected abstract void OnDone(byte[] data, float time, DataSegment segment);
    protected abstract void OnContinueText(byte[] data);
    protected abstract void OnContinueBinary(byte[] data);
    
    //<•······················•<>•······················•>

    protected void SetHttpState(HttpState state) => CurrentHttpState = state;
}