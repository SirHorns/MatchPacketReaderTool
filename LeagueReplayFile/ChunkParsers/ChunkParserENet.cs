using System.Text;
using LeagueReplayFile.Models;
using LeagueReplayFile.Protocols.ENet;
using LeagueReplayFile.Protocols.ENet.Protocols;

namespace LeagueReplayFile.ChunkParsers;

/// <summary>
/// Chunk parser that handles chunks sent over ENet protocols
/// </summary>
public class ChunkParserENet : ENetProtocolHandler, IChunkParser
{
    protected class FragmentBuffer
    {
        public int nextReliableSequenceNumber = 0;
        public int FragmentCount = 0;
        public int FragmentsLeft = 0;
        public byte[] Buffer = Array.Empty<byte>();
    }

    private BlowFish _blowfish { get; }


    public List<DataSegment> Segments { get; } = new();
    public List<ENetPacket> Packets { get; } = new();
    public ENetGameClientVersions Version { get; }

    public ChunkParserENet(ENetGameClientVersions version, byte[] key)
    {
        Version = version;
        _blowfish = new BlowFish(key);
    }
    
    

    private void AddPacket(byte[] data, float time, byte channel, ENetPacketFlags flags)
    {
        if (channel <= 7)
        {
            data = _blowfish.Decrypt(data);
        }

        if (data.Length == 0)
        {
            // Skipping None ENetPacket in Stream
            // TODO: Implement tracking of these.
            return; 
        }
        
        if (data[0] == 0xFF && channel > 0 && channel < 5)
        {
            using var reader = new BinaryReader(new MemoryStream(data));
            Ubatch(channel, reader, flags, time);
        }
        else
        {
            Packets.Add(new ENetPacket
            {
                Channel = channel,
                Bytes = data,
                Flags = flags,
                Time = time,
            });
        }
    }

    private void Ubatch(byte channel, BinaryReader reader, ENetPacketFlags flags, float time)
    {
        reader.ReadByte();
        int count = reader.ReadByte();
        if ((reader.BaseStream.Length) < 3 || count == 0)
        {
            return;
        }

        byte packetSize = 0;
        byte packetLastID = 0;
        int packetLastNetID = 0;
        byte[] packetData = null;
        for (int i = 0; i < count; i++)
        {
            packetSize = reader.ReadByte();
            if (i == 0)
            {
                packetLastID = reader.ReadByte();
                packetLastNetID = reader.ReadInt32();
                packetData = reader.ReadExactBytes(packetSize - 5);
            }
            else
            {
                if ((packetSize & 1) == 0) //if this is true re-use old packetID
                {
                    packetLastID = reader.ReadByte();
                }

                if ((packetSize & 2) == 0)
                {
                    packetLastNetID = reader.ReadInt32();
                }
                else
                {
                    packetLastNetID += reader.ReadSByte();
                }

                if ((packetSize >> 2) == 0x3F)
                {
                    packetSize = reader.ReadByte();
                }
                else
                {
                    packetSize = (byte)(packetSize >> 2);
                }

                packetData = reader.ReadExactBytes(packetSize);
            }

            using (var stream = new MemoryStream())
            {
                using (var packetWriter = new BinaryWriter(stream, Encoding.UTF8, true))
                {
                    packetWriter.Write(packetLastID);
                    packetWriter.Write(packetLastNetID);
                    packetWriter.Write(packetData);
                }

                stream.Seek(0, SeekOrigin.Begin);
                using (var packetReader = new BinaryReader(stream, Encoding.UTF8, true))
                {
                    Packets.Add(new ENetPacket
                    {
                        Channel = channel,
                        Bytes = packetReader.ReadExactBytes((int)stream.Length),
                        Flags = flags,
                        Time = time,
                    });
                }
            }
        }
    }

    private bool Handle(ENetProtocol command, ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader commandHeader)
    {
        return true;
    }

    private bool Handle(ENetProtocolSendReliable command, ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader commandHeader)
    {
        AddPacket(command.Data, protocolHeader.TimeRecieved, commandHeader.Channel, ENetPacketFlags.Reliable);
        return true;
    }

    private bool Handle(ENetProtocolSendUnsequenced command, ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader commandHeader)
    {
        AddPacket(command.Data, protocolHeader.TimeRecieved, commandHeader.Channel, ENetPacketFlags.Unsequenced);
        return true;
    }

    private bool Handle(ENetProtocolSendUnreliable command, ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader commandHeader)
    {
        AddPacket(command.Data, protocolHeader.TimeRecieved, commandHeader.Channel, ENetPacketFlags.None);
        return true;
    }

    private bool Handle(ENetProtocolSendFragment command, ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader commandHeader)
    {
        if (command.FragmentOffset >= command.TotalLength ||
            command.FragmentOffset + command.Data.Length > command.TotalLength ||
            command.FragmentNumber >= command.FragmentCount)
        {
            return false;
        }

        var channel = ChannelFragmentBuffer[commandHeader.Channel];
        FragmentBuffer buffer;
        if (channel.TryGetValue(command.StartSequenceNumber, out var value))
        {
            buffer = value;
        }
        else
        {
            if (command.StartSequenceNumber != commandHeader.ReliableSequenceNumber)
            {
                return true;
            }

            buffer = new FragmentBuffer
            {
                Buffer = new byte[command.TotalLength],
                FragmentCount = (int)command.FragmentCount,
                nextReliableSequenceNumber = commandHeader.ReliableSequenceNumber,
                FragmentsLeft = (int)command.FragmentCount
            };
            channel[command.StartSequenceNumber] = buffer;
        }

        if (buffer.nextReliableSequenceNumber != commandHeader.ReliableSequenceNumber)
        {
            return true;
        }

        if (buffer.FragmentCount != command.FragmentCount)
        {
            return false;
        }

        if (buffer.Buffer.Length != command.TotalLength)
        {
            return false;
        }

        buffer.nextReliableSequenceNumber++;
        buffer.FragmentsLeft--;

        Buffer.BlockCopy(command.Data, 0, buffer.Buffer, (int)command.FragmentOffset, command.Data.Length);
        if (buffer.FragmentsLeft <= 0)
        {
            AddPacket(buffer.Buffer, protocolHeader.TimeRecieved, commandHeader.Channel, ENetPacketFlags.Reliable);
            channel.Remove(command.StartSequenceNumber);
        }

        return true;
    }

    private static Dictionary<byte, Dictionary<ushort, FragmentBuffer>> MakeChannelBuffers()
    {
        var tmp = new Dictionary<byte, Dictionary<ushort, FragmentBuffer>>();
        for (int i = 0; i < 255; i++)
        {
            tmp[(byte)i] = new Dictionary<ushort, FragmentBuffer>();
        }

        return tmp;
    }
    
    protected Dictionary<byte, Dictionary<ushort, FragmentBuffer>> ChannelFragmentBuffer = MakeChannelBuffers();

    //

    /// <summary>
    /// Read the data stream from the ENet Chunk replay
    /// </summary>
    /// <param name="data"></param>
    public void Read(byte[] data)
    {
        // Read "segments" from stream and hand them over to parser
        var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var t = reader.ReadSingle();
            var l = reader.ReadInt32();
            var d = reader.ReadExactBytes(l);
            var p = reader.ReadByte();
        
            var segment =  new DataSegment()
            {
                Time = t,
                Length = l,
                Data = d,
                Pad = p
            };
            Segments.Add(segment);
        }

        foreach (var segment in Segments)
        {
            ReadSegment(segment);
        }
    }
    
    /// <summary>
    /// 
    /// </summary>
    /// <param name="segment"></param>
    private void ReadSegment(DataSegment segment)
    {
        var data = segment.Data;
        var time = segment.Time;
        var stream = new MemoryStream(data);
        using var reader = new BinaryReader(stream);
        Read(reader, time, Version);
    }
    
    // overrides

    protected override bool HandleProtocol(ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader protocolCommandHeader, ENetProtocol protocol)
    {
        dynamic dinProtocol = protocol;
        return Handle(dinProtocol, protocolHeader, protocolCommandHeader);
    }
}