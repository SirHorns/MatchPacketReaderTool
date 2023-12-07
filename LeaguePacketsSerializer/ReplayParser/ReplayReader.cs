using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LeaguePacketsSerializer;

namespace ENetUnpack.ReplayParser
{
    public class ReplayReader
    {
        [JsonProperty("replayVersion")]
        public string ReplayVersion { get; set; }

        [JsonProperty("clientVersion")]
        public string ClientVersion { get; set; }

        [JsonProperty("clientHash")]
        public string ClientHash { get; set; }

        [JsonProperty("encryptionKey")]
        public byte[] EncryptionKey { get; set; }

        [JsonProperty("matchID")]
        public int MatchID { get; set; }

        [JsonProperty("spectatorMode")]
        public bool SpectatorMode { get; set; }

        [JsonProperty("dataIndex")]
        public List<KeyValuePair<string, DataOffset>> DataIndex { get; set; }
        
        public static Replay ReadPackets(Stream stream, ENetLeagueVersion? enetLeagueVersion, ref JObject metadata)
        {
            
            using var reader = new BinaryReader(stream);
            
            // Basic header
            var _unused = reader.ReadByte();
            var _version = reader.ReadByte();
            var _compressed = reader.ReadByte();
            var _reserved = reader.ReadByte();

            if(_unused == 'n' && _version == 'f' && _compressed == 'o' && _reserved == '\0')
            {
                return NFO(reader);
            } 
            
            var replay = new Replay();
            
            
            // JSON data
            var jsonLength = reader.ReadInt32();
            var json = reader.ReadExactBytes(jsonLength);
            var jsonString = Encoding.UTF8.GetString(json);
            replay.MetaData = JsonConvert.DeserializeObject<MetaData>(jsonString);
            metadata = JObject.Parse(jsonString); //TODO: Extend Replay return it?
            var _replay = metadata.ToObject<ReplayReader>();

            
            
            
            
            // Binary data offset start position
            var _offsetStart = stream.Position;

            // Stream data
            var _stream = _replay.DataIndex.First(kvp => kvp.Key == "stream").Value;
            var _data = reader.ReadExactBytes(_stream.Size);

            if((_data[0] & 0x4C) != 0)
            {
                _data = BDODecompress.Decompress(_data);
            }

            // FIXME: detect correct league version
            // TODO: determing where exact breaking changes in league ENet are??
            var _enetLeagueVersion = enetLeagueVersion ?? ENetLeagueVersion.Seasson12;

            // Type of parser spectator or ingame/ENet
            IChunkParser _chunkParser = null;
            if(_replay.SpectatorMode)
            {
                _chunkParser = new ChunkParserSpectator(_replay.EncryptionKey, _replay.MatchID);
            }
            else
            {                    
                _chunkParser = new ChunkParserENet(_enetLeagueVersion, _replay.EncryptionKey);
            }

            // Read "chunks" from stream and hand them over to parser
            using (var chunksReader = new BinaryReader(new MemoryStream(_data)))
            {
                while (chunksReader.BaseStream.Position < chunksReader.BaseStream.Length)
                {
                    var _chunkTime = chunksReader.ReadSingle();
                    var _chunkLength = chunksReader.ReadInt32();
                    var _chunkData = chunksReader.ReadExactBytes(_chunkLength);
                    _chunkParser.Read(_chunkData, _chunkTime);
                    var _chunkUnk = chunksReader.ReadByte();
                }
            }

            replay.Packets = _chunkParser.Packets;
            return replay;
        }


        private static Replay NFO(BinaryReader reader)
        {
            var replay = new Replay();
            replay.Packets = new List<ENetPacket>();

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

                    replay.Packets.Add(new ENetPacket()
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
