using System;
using System.IO;
using LeaguePackets.LoadScreen;

namespace LeaguePackets;

public abstract class BasePacket
{
    public string PacketType { get; set; }

    public BasePacket()
    {
        var type = GetType();
        PacketType = type.Name;
    }

    public byte[] ExtraBytes { get; set; } = Array.Empty<byte>();

    protected abstract void ReadHeader(ByteReader reader);
    
    protected abstract void ReadBody(ByteReader reader);
        
    public void Read(byte[] data)
    {
        using var reader = new ByteReader(data);
        ReadHeader(reader);
        ReadBody(reader);
        this.ExtraBytes = reader.ReadLeft();
    }

    protected abstract void WriteHeader(ByteWriter writer);
    
    protected abstract void WriteBody(ByteWriter writer);
    
    public byte[] GetBytes()
    {
        using(var writer = new ByteWriter())
        {
            WriteHeader(writer);
            WriteBody(writer);
            writer.WriteBytes(this.ExtraBytes);
            return writer.GetBytes();
        }
    }
    
    public static BasePacket Create(byte[] data, ChannelID channel)
    {
        switch (channel)
        {
            case ChannelID.Default:
                return KeyCheckPacket.Create(data);
            case ChannelID.ClientToServer:
            case ChannelID.SynchClock:
            case ChannelID.Broadcast:
            case ChannelID.BroadcastUnreliable:
                return GamePacket.Create(data);
            case ChannelID.Chat: {
                var packet = new Chat();
                packet.Read(data);
                return packet;
            }
            case ChannelID.QuickChat:{
                var packet = new QuickChat();
                packet.Read(data);
                return packet;
            }
            case ChannelID.LoadingScreen:
                return LoadScreenPacket.Create(data);
            default:
                // Sometimes we get packets with unknown channels
                // Their use or what they do is unknown as of now
                return UnknownPacket.Create(data);
        }
    }
}