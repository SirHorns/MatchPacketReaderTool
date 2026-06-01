namespace LeagueReplayFile.Maestro;

public class MaestroMessage
{
    public int MaestroSize { get; set; } = 16;
    public int Unknown2 { get;  set; }
    public virtual MessageType Type { get; set; }
    public int DataLength { get;  set; }
    public byte[] Bytes { get;  set; } = [];
    public byte[] ExtraBytes { get;  set; } = [];
    
    public static MaestroMessage Read(byte[] data)
    {
        using var reader = new BinaryReader(new MemoryStream(data));
        var res = new MaestroMessage()
        {
            MaestroSize = reader.ReadInt32(),
            Unknown2 = reader.ReadInt32(),
            Type = (MessageType)reader.ReadInt32(),
            DataLength = reader.ReadInt32(),
            Bytes = data,
        };
        return res;
    }
    public static MaestroMessage Read(BinaryReader reader)
    {
        var res = new MaestroMessage()
        {
            MaestroSize = reader.ReadInt32(),
            Unknown2 = reader.ReadInt32(),
            Type = (MessageType)reader.ReadInt32(),
            DataLength = reader.ReadInt32(),
        };
        return res;
    }
}