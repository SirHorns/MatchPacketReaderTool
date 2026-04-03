namespace LeagueReplayFile.Protocols.ENet;

//TODO: Refactor how ENetPackets get serialized into league packets so packest structure is preserved better instead of being rearranged and dumped into Byts property
public class ENetPacket
{
    public float Time { get; set; }
    public byte[] Bytes { get; set; }
    public byte[] Data { get; set; }
    public byte Channel { get; set; }
    public ENetPacketFlags Flags { get; set; }
    public byte StructureFlags { get; set; }
    public byte Marker { get; set; }
    public int BlockParam { get; set; }
    public byte PacketTypeID { get; set; }
    
    public static ENetPacket Read(BinaryReader reader)
    {
        var time = 0.0f;
        byte packetType = 0;
        var blockParam = 0;
        byte marker = reader.ReadByte();
        byte flags = (byte)(marker >> 4);
        byte channel = (byte)(marker & 0x0F);
        int length;
        
        var w = (flags & 0x8) == 0;
        var x = (flags & 0x1) == 0;
        var y = (flags & 0x4) == 0;
        var z = (flags & 0x2) == 0;

        if (w)
        {
            time = reader.ReadSingle();
        }
        else
        {
            time += reader.ReadByte() / 1000.0f;
        }

        if (x)
        {
            length = reader.ReadInt32();
        }
        else
        {
            length = reader.ReadByte();
        }

        if (y)
        {
            packetType = reader.ReadByte();
        }

        if (z)
        {
            blockParam = reader.ReadInt32();
        }
        else
        {
            blockParam += reader.ReadByte();
        }

        var data = reader.ReadExactBytes(length);
        
        var buffer = new List<byte> { packetType };
        buffer.AddRange(BitConverter.GetBytes(blockParam));
        buffer.AddRange(data);
        var pkt = new ENetPacket
        {
            PacketTypeID = packetType,
            Channel = channel,
            Bytes = buffer.ToArray(),
            Data = data,
            Flags = (ENetPacketFlags)flags,
            Time = time,
            Marker = marker,
            StructureFlags = flags,
            BlockParam = blockParam
        };

        return pkt;
    }
    
    public static void Write(ENetPacket packet, BinaryWriter writer)
    {
        var flags = packet.StructureFlags;
        var w = (flags & 0x8) == 0;
        var y = (flags & 0x4) == 0;
        
        writer.Write(packet.Marker);

        if (w)
        {
            writer.Write(packet.Time);
        }
        else
        {
            writer.Write(packet.Time * 1000);
        }
        writer.Write(packet.Bytes.Length);
        if (y)
        {
            writer.Write(packet.PacketTypeID);
        }
        writer.Write(packet.BlockParam);
        writer.Write(packet.Data);
    }
}