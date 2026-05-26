using LeagueReplayFile.Enums;
using LeagueReplayFile.Models;
using LeagueReplayFile.Models.Sections;
using LeagueReplayFile.Parsers;
using LeagueReplayFile.Protocols.ENet;
using ENetPacketFlags = LeagueReplayFile.Protocols.ENet.ENetPacketFlags;

namespace LeagueReplayFile;

public static class LRFReaderExtensions
{
    public static void Parse(this LRFReader reader)
    {
        var stream = reader.Stream;
        var offsetStart = stream.Position;
        var metaData = reader.LRF.MetaData;

        // Stream data
        var streamOffset = metaData.DataIndex.First(kvp => kvp.Key == "stream").Value;
        var data = reader.BinaryReader.ReadExactBytes(streamOffset.Size);
        if ((data[0] & 0x4C) != 0)
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
            var sections = ParseSpectator(metaData, data);
            reader.LRF.Sections = sections;
        }
        else if (metaData.IsStream)
        {
            Console.WriteLine("Stream");
            var packets = ParseStream(version, metaData, data);
            reader.LRF.Packets = packets;
        }
        else if (metaData.ObserverStream)
        {
            Console.WriteLine("ObserverStream");
            var packets = ParseStream(version, metaData, data);
            reader.LRF.Packets = packets;
        }
        else
        {
            Console.WriteLine("POVStream");
            var packets = ParseStream(version, metaData, data);
            reader.LRF.Packets = packets;
        }


        if (metaData.SpectatorMode)
        {
            reader.LRF.Type = LRFTypes.HTTP;
        }
        else
        {
            reader.LRF.Type = LRFTypes.ENET;
        }
    }

    public static void ParseNFO(this LRFReader reader)
    {
        var rawPackets = new List<ENetPacket>();
        var br = reader.BinaryReader;
        while (br.BaseStream.Position < br.BaseStream.Length)
        {
            var dataSize = (int)br.ReadUInt32();
            var time = br.ReadSingle();
            var channel = br.ReadByte();
            var reserved = br.ReadExactBytes(3);

            if (dataSize == 0)
            {
                continue;
            }

            var pktData = br.ReadExactBytes(dataSize);

            rawPackets.Add(new ENetPacket()
            {
                Time = time,
                Bytes = pktData,
                Channel = channel,
                Flags = ENetPacketFlags.None
            });

            var remain = dataSize % 16;
            if (remain != 0)
            {
                br.BaseStream.Seek(16 - remain, SeekOrigin.Current);
            }
        }
        reader.LRF.Packets = rawPackets;
    }

    public static List<Section> ParseSpectator(ReplayMetaData metaData, byte[] data)
    {
        var parser = new HttpReplayParser(metaData.EncryptionKey, metaData.MatchId);
        parser.Read(data);
        parser.Parse();
        return parser.Sections;
    }

    public static List<ENetPacket> ParseStream(ENetGameClientVersions version, ReplayMetaData metaData, byte[] data)
    {
        var parser = new StreamReplayParser(version, metaData.EncryptionKey);
        parser.Read(data);
        return parser.Packets;
    }
}