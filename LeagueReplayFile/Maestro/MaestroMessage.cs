namespace LeagueReplayFile.Maestro;

public class MaestroMessage
{
    public int Unknown1 { get; set; } = 16;
    public int Unknown2 { get;  set; }
    public virtual MessageType Type { get; set; }
    public int DataLength { get;  set; }
    public byte[] Bytes { get;  set; } = [];
    public byte[] ExtraBytes { get;  set; } = [];
    
    public static MaestroMessage Read(byte[] data)
    {
        var reader = new BinaryReader(new MemoryStream(data));
        var res = new MaestroMessage()
        {
            Unknown1 = reader.ReadInt32(),
            Unknown2 = reader.ReadInt32(),
            Type = (MessageType)reader.ReadInt32(),
            DataLength = reader.ReadInt32(),
            Bytes = data,
        };
        return res;
    }
}