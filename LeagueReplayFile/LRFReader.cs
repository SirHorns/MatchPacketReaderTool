using System.Text;
using LeagueReplayFile.ChunkParsers;
using LeagueReplayFile.Enums;
using LeagueReplayFile.Protocols.ENet;
using LeagueReplayFile.Structs;
using Newtonsoft.Json;
using ENetPacketFlags = LeagueReplayFile.Protocols.ENet.ENetPacketFlags;

namespace LeagueReplayFile;

public class LRFReader: IDisposable
{
    private BinaryReader? _reader;
    
    public LRF? Read(Stream stream)
    {
        _reader = new BinaryReader(stream);
        var header = ReadHeader();
        LRF? lrf = new LRF()
        {
            Type = LRFTypes.NAN,
            Stream = stream,
            BasicHeader = header
        };
        MetaData metaData = null;
        LRFTypes type;
        if(header.Unused == 'n' && header.Version == 'f' && header.Compressed == 'o' && header.Reserved == '\0')
        {
            type = LRFTypes.NFO; // LRF is a NFO replay
        }
        else
        {
            metaData = ReadMetaData();
            if (metaData.SpectatorMode)
            {
                type = LRFTypes.SPECTATOR;
            }
            else
            {
                type = LRFTypes.ENET;
            }
        }


        if (type == LRFTypes.NFO)
        {
            var nfo = Encoding.UTF8.GetString(_reader.ReadExactBytes(4)) == "nfo";
            var dataSize = (int)_reader.ReadUInt32();
            if(nfo)
            {
                metaData = ReadMetaDataNFO(dataSize); 
            }
            Console.WriteLine($"{type}");
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
            var majorVersion = Int32.Parse(clientVersion.Split('.')[0]);
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
            
            Console.WriteLine($"{type} - {metaData.ClientVersion}");
        
            if (type is LRFTypes.SPECTATOR)
            {
                var parser = new ChunkParserSpectator(metaData.EncryptionKey, metaData.MatchId);
                parser.Read(data);
                lrf.Sections = parser.Sections;
            }
            else if (type is LRFTypes.ENET)
            {
                var parser = new ChunkParserENet(version, metaData.EncryptionKey);
                //parser.Read(data);
            }
        }

        lrf.Type = type;
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

    public MetaData ReadMetaData()
    {
        var jsonLength = _reader.ReadInt32();
        var json = _reader.ReadExactBytes(jsonLength);
        var jsonString = Encoding.UTF8.GetString(json);
        return JsonConvert.DeserializeObject<MetaData>(jsonString);
    }
    
    public MetaData ReadMetaDataNFO(int dataSize)
    {
        var pad = _reader.ReadUInt64();
        var jsonData = _reader.ReadExactBytes(dataSize);
        var jsonString = Encoding.UTF8.GetString(jsonData);
        return JsonConvert.DeserializeObject<MetaData>(jsonString);
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

    private void Specator()
    {
        
    }
    

    public void Dispose()
    {
        _reader?.Dispose();
    }
}