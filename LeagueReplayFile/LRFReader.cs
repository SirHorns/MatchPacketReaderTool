using System.Text;
using LeagueReplayFile.ChunkParsers;
using LeagueReplayFile.Enums;
using LeagueReplayFile.Models;
using LeagueReplayFile.Models.Sections;
using LeagueReplayFile.Protocols.ENet;
using Newtonsoft.Json;
using ENetPacketFlags = LeagueReplayFile.Protocols.ENet.ENetPacketFlags;

namespace LeagueReplayFile;

/// <summary>
/// Reads LRF file contents into a C# Object
/// </summary>
public class LRFReader: IDisposable
{
    private BinaryReader? _reader;
    
    public LRF? Read(Stream stream)
    {
        _reader = new BinaryReader(stream);
        var header = ReadHeader();
        var lrf = new LRF()
        {
            Type = LRFTypes.NAN,
            Stream = stream,
            BasicHeader = header
        };
        ReplayMetaData? metaData = null;
        var isNFO = header.Unused == 'n' && header.Version == 'f' && header.Compressed == 'o' &&
                    header.Reserved == '\0';

        if (isNFO)
        {
            var nfo = Encoding.UTF8.GetString(_reader.ReadExactBytes(4)) == "nfo";
        }
        var dataSize = _reader.ReadUInt32();
        if (isNFO)
        {
            var pad = _reader.ReadUInt64();
        }
        metaData = ReadMetaData((int)dataSize);
        Console.WriteLine($"Client Version: {metaData.ClientVersion}");
        Console.WriteLine($"Replay Version: {metaData.ReplayVersion}");
        if (isNFO)
        {
            var packets = NFO();
            lrf.Packets = packets;
        } 
        else
        {
            var offsetStart = stream.Position;

            // Stream data
            var streamOffset = metaData.DataIndex.First(kvp => kvp.Key == "stream").Value;
            var data = _reader.ReadExactBytes(streamOffset.Size);
            if((data[0] & 0x4C) != 0)
            {
                data = BDODecompress.Decompress(data);
            }
            
            ENetGameClientVersions version;
            var clientVersion = metaData.ClientVersion;
            var majorVersion = int.Parse(clientVersion.Split('.')[0]);
            switch (majorVersion)
            { 
                case 1:
                    version = ENetGameClientVersions.Patch1;
                    break;
                case 2:
                    version = ENetGameClientVersions.Patch2;
                    break;
                case 3:
                    version = ENetGameClientVersions.Patch3;
                    break;
                case 4:
                    version = ENetGameClientVersions.Patch4;
                    break;
                case 5:
                    version = ENetGameClientVersions.Patch5;
                    break;
                case 6:
                    version = ENetGameClientVersions.Patch6;
                    break;
                case 7:
                    version = ENetGameClientVersions.Patch7;
                    break;
                case 8:
                    version = ENetGameClientVersions.Patch8;
                    break;
                case < 8:
                default:
                    version = ENetGameClientVersions.Unknown;
                    break;
            }
        
            if (metaData.SpectatorMode)
            {
                Console.WriteLine("SpectatorMode");
                var sections = Spectator(metaData, data);
                lrf.Sections = sections;
            }
            else if (metaData.IsStream)
            {
                Console.WriteLine("Stream");
                var packets = Stream(version, metaData, data);
                lrf.Packets = packets;
            }
            else if (metaData.ObserverStream)
            {
                Console.WriteLine("ObserverStream");
                var packets = Stream(version, metaData, data);
                lrf.Packets = packets;
            }
            else
            {
                Console.WriteLine("POVStream");
                var packets = Stream(version, metaData, data);
                lrf.Packets = packets;
            }
        }

        if (isNFO)
        {
            lrf.Type = LRFTypes.NFO; // LRF is a NFO replay
        }
        else
        {
            if (metaData.SpectatorMode)
            {
                lrf.Type = LRFTypes.HTTP;
            }
            else
            {
                lrf.Type = LRFTypes.ENET;
            }
        }
        lrf.MetaData = metaData;
        return lrf;
    }
    
    
    
    private BasicHeader ReadHeader()
    {
        return new BasicHeader()
        {
            Unused = _reader.ReadByte(),
            Version = _reader.ReadByte(),
            Compressed = _reader.ReadByte(),
            Reserved = _reader.ReadByte()
        };
    }

    public ReplayMetaData? ReadMetaData(int dataSize)
    {
        var bytes = _reader.ReadExactBytes(dataSize);
        var json = Encoding.UTF8.GetString(bytes);
        var metadata = JsonConvert.DeserializeObject<ReplayMetaData>(json);
        return metadata;
    }
    
    private List<ENetPacket> NFO()
    {
        var rawPackets = new List<ENetPacket>();
        while(_reader.BaseStream.Position < _reader.BaseStream.Length)
        {
            var dataSize = (int)_reader.ReadUInt32();
            var time = _reader.ReadSingle();
            var channel = _reader.ReadByte();
            var reserved = _reader.ReadExactBytes(3);

            if(dataSize == 0)
            {
                continue;
            }

            var pktData = _reader.ReadExactBytes(dataSize);

            rawPackets.Add(new ENetPacket()
            {
                Time = time,
                Bytes = pktData,
                Channel = channel,
                Flags = ENetPacketFlags.None
            });

            var remain = dataSize % 16;
            if(remain != 0)
            {
                _reader.BaseStream.Seek(16 - remain, SeekOrigin.Current);
            }
        }

        return rawPackets;
    }

    private List<Section> Spectator(ReplayMetaData metaData, byte[] data)
    {
        var parser = new HttpReplayParser(metaData.EncryptionKey, metaData.MatchId);
        parser.Read(data);
        return parser.Sections;
    }
    
    private List<ENetPacket> Stream(ENetGameClientVersions version, ReplayMetaData metaData, byte[] data)
    {
        var parser = new StreamReplayParser(version, metaData.EncryptionKey);
        parser.Read(data);
        return parser.Packets;
    }
    
    public void Dispose()
    {
        _reader?.Dispose();
    }
}