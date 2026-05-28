using System.Text.RegularExpressions;
using LeagueReplayFile.Models;

namespace LeagueReplayFile.Protocols;

public abstract class HttpProtocol
{
    protected Regex RE_CONTENT_LEN = new("Content-Length: ([0-9]+)", RegexOptions.IgnoreCase);

    protected byte[] HTTP_END = { 0x0D, 0x0A, 0x0D, 0x0A };
    
    protected HttpProtocol() { }
    
    //<•······················•<>•······················•>

    protected abstract void Http(byte[] data);
    protected abstract void Get(string request);
    protected abstract void Post(string request);
    protected abstract void Head(byte[] data, float time);
    protected abstract void Options(byte[] data, float time);
    
    //<•······················•<>•······················•>
    
    protected abstract void OnGetBinary(byte[] data);
    protected abstract void OnGetText(byte[] data);
    protected abstract void OnDone(byte[] data, float time, DataSegment segment);
    protected abstract void OnContinueText(byte[] data);
    protected abstract void OnContinueBinary(byte[] data);
}