namespace LeagueReplayFileSerializer.Data;

public class BadPacket
{
    public int RawID { get; set; }
    public byte[] Raw { get; set; }
    public byte RawChannel { get; set; }
    public string Error { get; set; }
}