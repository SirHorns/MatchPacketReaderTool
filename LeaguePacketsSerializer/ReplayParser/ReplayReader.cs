using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LeaguePacketsSerializer;

namespace ENetUnpack.ReplayParser
{
    public class ReplayReader
    {
        public Replay Replay { get; private set; }

        public void ReadPackets(Stream stream, ENetLeagueVersion? enetLeagueVersion)
        {
            using var reader = new BinaryReader(stream);
            
            // Basic header
            var unused = reader.ReadByte();
            var version = reader.ReadByte();
            var compressed = reader.ReadByte();
            var reserved = reader.ReadByte();

            if(unused == 'n' && version == 'f' && compressed == 'o' && reserved == '\0')
            {
                Replay = Nfo(reader);
                return;
            } 
            
            Replay = new Replay();
            
            // JSON data
            var jsonLength = reader.ReadInt32();
            var json = reader.ReadExactBytes(jsonLength);
            var jsonString = Encoding.UTF8.GetString(json);
            Replay.MetaData = JsonConvert.DeserializeObject<MetaData>(jsonString);
            //TODO: Extend Replay return it?
            
            // Binary data offset start position
            var offsetStart = stream.Position;

            // Stream data
            var dataOffset = Replay.MetaData.DataIndex.First(kvp => kvp.Key == "stream").Value;
            var data = reader.ReadExactBytes(dataOffset.Size);

            if((data[0] & 0x4C) != 0)
            {
                data = BDODecompress.Decompress(data);
            }

            // FIXME: detect correct league version
            // TODO: determining where exact breaking changes in league ENet are??
            var eNetLeagueVersion = enetLeagueVersion ?? ENetLeagueVersion.Seasson12;

            // Type of parser spectator or in-game/ENet
            IChunkParser chunkParser = null;
            if(Replay.MetaData.SpectatorMode)
            {
                chunkParser = new ChunkParserSpectator(Replay.MetaData.EncryptionKey, Replay.MetaData.MatchId);
            }
            else
            {                    
                chunkParser = new ChunkParserENet(eNetLeagueVersion, Replay.MetaData.EncryptionKey);
            }

            // Read "chunks" from stream and hand them over to parser
            using (var chunksReader = new BinaryReader(new MemoryStream(data)))
            {
                while (chunksReader.BaseStream.Position < chunksReader.BaseStream.Length)
                {
                    var chunkTime = chunksReader.ReadSingle();
                    var chunkLength = chunksReader.ReadInt32();
                    var chunkData = chunksReader.ReadExactBytes(chunkLength);
                    chunkParser.Read(chunkData, chunkTime);
                    var chunkUnk = chunksReader.ReadByte();
                }
            }

            Replay.RawPackets = chunkParser.Packets;
        }


        private static Replay Nfo(BinaryReader reader)
        {
            var replay = new Replay
            {
                RawPackets = new List<ENetPacket>()
            };

            var first = true;
            while(reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var nfo = first || Encoding.UTF8.GetString(reader.ReadExactBytes(4)) == "nfo";
                first = false;

                var dataSize = (int)reader.ReadUInt32();
                if(nfo)
                {
                    var pad = reader.ReadUInt64();
                    var jsonData = reader.ReadExactBytes(dataSize);
                    var jsonString = Encoding.UTF8.GetString(jsonData);
                    replay.MetaData = JsonConvert.DeserializeObject<MetaData>(jsonString); //JObject.Parse(jsonString); //TODO: Extend Replay return it?
                }
                else
                {
                    var time = reader.ReadSingle();
                    var channel = reader.ReadByte();
                    var reserved = reader.ReadExactBytes(3);

                    if(dataSize == 0)
                    {
                        continue;
                    }

                    var pktData = reader.ReadExactBytes(dataSize);

                    replay.RawPackets.Add(new ENetPacket()
                    {
                        Time = time,
                        Bytes = pktData,
                        Channel = channel,
                        Flags = ENetPacketFlags.None
                    });
                }

                var remain = dataSize % 16;
                if(remain != 0)
                {
                    reader.BaseStream.Seek(16 - remain, SeekOrigin.Current);
                }
            }

            return replay;
        }
    }
}
