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


    protected void ReadSegment(DataSegment segment)
    {
        var data = segment.Data;
        var time = segment.Time;

        switch (_httpState)
        {
            case HttpState.GetBinary:
                HandleGetBinary(data);
                break;
            case HttpState.GetText:
                HandleGetText(data);
                break;
            case HttpState.Done:
                HandleDone(data, time, segment);
                break;
            case HttpState.ContinueBinary:
                HandleContinueBinary(data);
                break;
            case HttpState.ContinueText:
                HandleContinueText(data);
                break;
            default:
                Console.WriteLine($"Skipped Segment: {_httpState}");
                break;
        }
    }
    
    //<•······················•<>•······················•>

    private void HandleGetBinary(byte[] data)
    {
        OnGetBinary(data);
        SetHttpState(HttpState.Done);
    }
    
    private void HandleGetText(byte[] data)
    {
        OnGetText(data);
        SetHttpState(HttpState.Done);
    }
    
    private void HandleDone(byte[] data, float time, DataSegment segment)
    {
        OnDone(data, time, segment);
    }
    
    private void HandleContinueBinary(byte[] data)
    {
        OnContinueBinary(data);
    }
    
    private void HandleContinueText(byte[] data)
    {
        OnContinueText(data);
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