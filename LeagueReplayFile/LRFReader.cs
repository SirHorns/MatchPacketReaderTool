using System.Text;
using LeagueReplayFile.Enums;
using LeagueReplayFile.Models;
using LeagueReplayFile.Models.Sections;
using LeagueReplayFile.Parsers;
using LeagueReplayFile.Protocols.ENet;
using Newtonsoft.Json;
using ENetPacketFlags = LeagueReplayFile.Protocols.ENet.ENetPacketFlags;

namespace LeagueReplayFile;

public static class LRFReader
{
    /// <summary>
    /// Reads a LRF file from a stream and returns a LRF object
    /// </summary>
    /// <param name="stream"></param>
    /// <returns></returns>
    /// <exception cref="NullReferenceException"></exception>
    public static LRF Read(Stream stream)
    {
        using var reader = new BinaryReader(stream);
        var header = ReadHeader(reader);
        MetaData? metaData = null;
        var isNfo = header.Unused == 'n' && header.Version == 'f' && header.Compressed == 'o' && header.Reserved == '\0';

        if (isNfo)
        {
            var nfo = Encoding.UTF8.GetString(reader.ReadExactBytes(4)) == "nfo";
            
        }
        var dataSize = reader.ReadUInt32();
        if (isNfo)
        {
            var pad = reader.ReadUInt64();
        }
        metaData = ReadMetaData(reader,(int)dataSize);
        if (metaData == null)
        {
            throw new NullReferenceException("Unable to read metadata!");
        }
        
        LRF lrf;
        if (isNfo)
        {
            lrf = new ENetLRF()
            {
                Type = LRFType.NFO,
                MetaData = metaData,
                Stream = stream,
                BasicHeader = header
            };
        }
        else if (metaData.SpectatorMode)
        {
            lrf = new SpectatorLRF()
            {
                Type = LRFType.HTTP,
                MetaData = metaData,
                Stream = stream,
                BasicHeader = header
            };
        }
        else
        {
            lrf = new ENetLRF()
            {
                Type = LRFType.ENET,
                MetaData = metaData,
                Stream = stream,
                BasicHeader = header
            };
        }
        if (isNfo)
        {
            NFO((ENetLRF)lrf, reader);
        } 
        else
        {
            var offsetStart = stream.Position;
            // Stream data
            var streamOffset = metaData.DataIndex.First(kvp => kvp.Key == "stream").Value;
            var data = reader.ReadExactBytes(streamOffset.Size);
            if((data[0] & 0x4C) != 0)
            {
                data = BDODecompress.Decompress(data);
            }
            
            if (metaData.SpectatorMode)
            {
               Spectator((SpectatorLRF)lrf, data);
            }
            else //if (metaData.IsStream || metaData.ObserverStream)
            {
                Stream((ENetLRF)lrf, data);
            }
        }
        
        return lrf;
    }

    /// <summary>
    /// Reads a LRF file from a stream and returns an LRF MetaData
    /// </summary>
    /// <param name="stream"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public static MetaData? Read(Stream stream, out LRFType type)
    {
        using var reader = new BinaryReader(stream);
        var header = ReadHeader(reader);

        MetaData? metaData = null;
        var isNFO = header.Unused == 'n' && header.Version == 'f' && header.Compressed == 'o' &&
                    header.Reserved == '\0';

        if (isNFO)
        {
            var nfo = Encoding.UTF8.GetString(reader.ReadExactBytes(4)) == "nfo";
        }
        var dataSize = reader.ReadUInt32();
        if (isNFO)
        {
            var pad = reader.ReadUInt64();
        }
        metaData = ReadMetaData(reader, (int)dataSize);
        
        if (isNFO)
        {
            type = LRFType.NFO; // LRF is a NFO replay
        }
        else
        {
            type = metaData.SpectatorMode ? LRFType.HTTP : LRFType.ENET;
        }
        
        return metaData;
    }
    
    //
    
    private static BasicHeader ReadHeader(BinaryReader reader)
    {
        return new BasicHeader()
        {
            Unused = reader.ReadByte(),
            Version = reader.ReadByte(),
            Compressed = reader.ReadByte(),
            Reserved = reader.ReadByte()
        };
    }

    private static MetaData? ReadMetaData(BinaryReader reader, int dataSize)
    {
        var bytes = reader.ReadExactBytes(dataSize);
        var json = Encoding.UTF8.GetString(bytes);
        var metadata = JsonConvert.DeserializeObject<MetaData>(json);
        return metadata;
    }
    
    private static void NFO(ENetLRF lrf, BinaryReader reader)
    {
        var rawPackets = new List<ENetPacket>();
        while(reader.BaseStream.Position < reader.BaseStream.Length)
        {
            var dataSize = (int)reader.ReadUInt32();
            var time = reader.ReadSingle();
            var channel = reader.ReadByte();
            var reserved = reader.ReadExactBytes(3);

            if(dataSize == 0)
            {
                continue;
            }

            var pktData = reader.ReadExactBytes(dataSize);

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
                reader.BaseStream.Seek(16 - remain, SeekOrigin.Current);
            }
        }
        lrf.Packets = rawPackets;
    }

    private static void Spectator(SpectatorLRF lrf, byte[] data)
    {
        var parser = new HttpReplayParser(lrf.MetaData.EncryptionKey, lrf.MetaData.MatchId);
        parser.Read(data);
        lrf.Sections = parser.Sections;
    }
    
    private static void Stream(ENetLRF lrf, byte[] data)
    {
        ENetGameClientVersions version;
        var clientVersion = lrf.MetaData.ClientVersion;
        if (string.IsNullOrEmpty(clientVersion))
        {
            version = ENetGameClientVersions.Unknown;
        }
        else
        {
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
        }
        var parser = new StreamReplayParser(version, lrf.MetaData.EncryptionKey);
        parser.Read(data);
        lrf.Packets = parser.Packets;
    }
}