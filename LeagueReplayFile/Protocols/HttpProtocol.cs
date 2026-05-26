using System.Text.RegularExpressions;
using LeagueReplayFile.Enums;
using LeagueReplayFile.Models;
using LeagueReplayFile.Models.Sections;

namespace LeagueReplayFile.Protocols;

public abstract class HttpProtocol
{
    protected static Regex RE_CONTENT_LEN = new("Content-Length: ([0-9]+)", RegexOptions.IgnoreCase);

    protected static byte[] HTTP_END = { 0x0D, 0x0A, 0x0D, 0x0A };
    
    
    private HttpState _httpState = HttpState.Done;
    
    
    protected List<byte> Buffer { get; }
    protected long BufferExpectedLength { get; set; }
    protected Section CurrentSection { get; set; }


    protected HttpProtocol()
    {
        Buffer = [];
        
    }


    protected void ParseSegment(DataSegment segment)
    {
        var data = segment.Data;
        var time = segment.Time;
        bool skipped = false;
        bool setDone = false;
        switch (_httpState)
        {
            case HttpState.GetBinary:
                OnGetBinary(data);
                setDone = true;
                break;
            case HttpState.GetText:
                OnGetText(data);
                setDone = true;
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
                skipped = true;
                break;
        }

        if (skipped)
        {
            Console.WriteLine($"Skipped Segment: {_httpState}");
        }

        if (setDone)
        {
            SetHttpState(HttpState.Done);
        }
        
    }
    
    //<•······················•<>•······················•>
    
    protected abstract void OnGetBinary(byte[] data);
    protected abstract void OnGetText(byte[] data);
    protected abstract void OnDone(byte[] data, float time, DataSegment segment);
    protected abstract void OnContinueText(byte[] data);
    protected abstract void OnContinueBinary(byte[] data);
    
    //<•······················•<>•······················•>

    protected void SetHttpState(HttpState state)
    {
        _httpState = state;
    }
}