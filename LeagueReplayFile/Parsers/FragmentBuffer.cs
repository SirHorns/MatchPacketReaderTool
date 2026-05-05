namespace LeagueReplayFile.Parsers;

public class FragmentBuffer
{
    public int nextReliableSequenceNumber = 0;
    public int FragmentCount = 0;
    public int FragmentsLeft = 0;
    public byte[] Buffer = Array.Empty<byte>();
}