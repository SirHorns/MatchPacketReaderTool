using System.IO.Compression;
using System.Text;
using LeagueReplayFile.Enums;
using LeagueReplayFile.Protocols;
using LeagueReplayFile.Protocols.ENet;
using LeagueReplayFile.Structs;
using LeagueReplayFile.Structs.Sections;
using ENetPacketFlags = LeagueReplayFile.Protocols.ENet.ENetPacketFlags;

namespace LeagueReplayFile.ChunkParsers;

/// <summary>
/// Chunk parser that handles chunks sent over HTTP
/// </summary>
public class ChunkParserSpectator : HttpProtocolHandler, IChunkParser
{
    private readonly BlowFish _blowfish;
    private BinaryReader _reader;
    public List<DataSegment> Segments { get; } = [];

    public List<ENetPacket> Packets { get; } = [];


    public ChunkParserSpectator(byte[] key, int matchId)
    {
        var keyBlowfish = new BlowFish(Encoding.ASCII.GetBytes(matchId.ToString()));
        _blowfish = new BlowFish(keyBlowfish.Decrypt(key).Take(16).ToArray());
    }

    public void Read(byte[] data)
    {
        _reader = new BinaryReader(new MemoryStream(data));

        while (_reader.BaseStream.Position < _reader.BaseStream.Length)
        {
            var t = _reader.ReadSingle();
            var l = _reader.ReadInt32();
            var d = _reader.ReadExactBytes(l);
            var p = _reader.ReadByte();
        
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

    //

    private List<ENetPacket> ReadSectionPackets(BinaryReader reader)
    {
        var time = 0.0f;
        byte packetType = 0;
        var blockParam = 0;

        var pkts = new List<ENetPacket>();

        while (reader.BaseStream.Position < reader.BaseStream.Length)
        {
            byte marker = reader.ReadByte();
            byte flags = (byte)(marker >> 4);
            byte channel = (byte)(marker & 0x0F);
            int length;

            if ((flags & 0x8) == 0)
            {
                time = reader.ReadSingle();
            }
            else
            {
                time += reader.ReadByte() / 1000.0f;
            }

            if ((flags & 0x1) == 0)
            {
                length = reader.ReadInt32();
            }
            else
            {
                length = reader.ReadByte();
            }

            if ((flags & 0x4) == 0)
            {
                packetType = reader.ReadByte();
            }

            if ((flags & 0x2) == 0)
            {
                blockParam = reader.ReadInt32();
            }
            else
            {
                blockParam += reader.ReadByte();
            }

            var packetData = reader.ReadExactBytes(length);
            var pkt = AddPacket(packetType, channel, blockParam, packetData, time);
            pkts.Add(pkt);
        }

        return pkts;
    }

    private ENetPacket AddPacket(byte packetType, byte channel, int blockParam, byte[] data, float time)
    {
        var buffer = new List<byte> { packetType };
        buffer.AddRange(BitConverter.GetBytes(blockParam));
        buffer.AddRange(data);
        var pkt = new ENetPacket
        {
            Channel = channel,
            Bytes = buffer.ToArray(),
            Flags = ENetPacketFlags.None,
            Time = time,
        };
        Packets.Add(pkt);
        return pkt;
    }

    protected override void HandleTextPacket(byte[] data)
    {
        switch (CurrentRequest)
        {
            case RequestTypes.VERSION:
                ((VersionSection)CurrentSection).Text = Encoding.UTF8.GetString(data);
                break;
            case RequestTypes.GAME_META_DATA:
                ((MetaDataSection)CurrentSection).Json = Encoding.UTF8.GetString(data);
                break;
            case RequestTypes.LAST_CHUNK_INFO:
                ((LastChunkInfoSection)CurrentSection).Json = Encoding.UTF8.GetString(data);
                break;
            case RequestTypes.KEY_FRAME:
                break;
            case RequestTypes.GAME_DATA_CHUNK:
            case RequestTypes.NONE:
            default:
                //
                break;
        }

        // probbly a better way to do this
        // but most non data chunks so far are at most a little over 800 bytes
        if (data.Length > 900)
        {
            HandleBinaryData(data);
        }
    }

    protected override void HandleBinaryData(byte[] data)
    {
        List<ENetPacket> pkts;
        var decrypted = _blowfish.Decrypt(data);
        using var decompressed = new MemoryStream();
        using (var compressed = new GZipStream(new MemoryStream(decrypted), CompressionMode.Decompress))
        {
            compressed.CopyTo(decompressed);
        }

        decompressed.Seek(0, SeekOrigin.Begin);
        using (var reader = new BinaryReader(decompressed))
        {
            pkts = ReadSectionPackets(reader);
        }

        switch (CurrentSection)
        {
            case GameDataSection gameDataSection:
                gameDataSection.Chunk.Packets.AddRange(pkts);
                break;
            case KeyFrameSection keyFrameSection:
                keyFrameSection.Packets.AddRange(pkts);
                break;
        }
    }
}