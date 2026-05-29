using System.Text;

namespace LeagueReplayFile;

public enum MessageTypes
{
    GAME_START = 0,
    GAME_END = 1,
    GAME_CRASHED = 2,
    CLOSE = 3,

    HEARTBEAT = 4,
    ACK = 5,

    GAMECLIENT_CREATE = 6,
    GAMECLIENT_ABANDONED = 7,

    GAMECLIENT_LAUNCHED = 8,
    GAMECLIENT_STOPPED = 9,
    GAMECLIENT_CONNECTED_TO_SERVER = 10,

    CHATMESSAGE_TO_GAME = 11,
    CHATMESSAGE_FROM_GAME = 12,
    /// <summary>
    /// Not used by riot. Created for tracking and debuging
    /// </summary>
    DUMMY
}

public class Message
{
    public int Identifier = 16;
    public int HeaderLength { get;  set; }
    public virtual MessageTypes Type { get; set; }
    public int DataLength { get;  set; }

    public byte[] ExtraBytes { get; set; } = [];

    public byte[] RawBytes { get;  set; } = [];

    public byte[] Data { get; set; } = [8];
}

public class Testing
{
    public static Message Read(byte[]? data)
    {
        var reader = new BinaryReader(new MemoryStream(data));
        var res = new Message()
        {
            Identifier = reader.ReadInt32(),
            HeaderLength = reader.ReadInt32(),
            Type = (MessageTypes)reader.ReadInt32(),
            DataLength = reader.ReadInt32(),
            RawBytes = data,
        };
        res.Data = res.DataLength == 0 ? new byte[4] : new byte[res.DataLength];
        res.Data = reader.ReadBytes(res.DataLength);
        int bytesLeft = (int)(reader.BaseStream.Length - reader.BaseStream.Position);
        res.ExtraBytes = reader.ReadBytes(bytesLeft);
        return res;
    }
}