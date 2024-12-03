using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using LeaguePacketsSerializer.Enums;

namespace LeaguePacketsSerializer.Parsers.ChunkParsers;

public class SpectatorChunkParser : HttpProtocolHandler, IChunkParser
    {
        private readonly BlowFish _blowfish;
        private BinaryReader _reader;
        private List<DataSegment> _segments;
        
        public List<ENetPacket> Packets { get; } =  new();

        
            
        public SpectatorChunkParser(byte[] key, int matchId)
        {
            var keyBlowfish = new BlowFish(Encoding.ASCII.GetBytes(matchId.ToString()));
            _blowfish = new BlowFish(keyBlowfish.Decrypt(key).Take(16).ToArray());
        }
        
        public void Read(byte[] data)
        {
            _segments = new List<DataSegment>();
            _reader = new BinaryReader(new MemoryStream(data));
            
            while (_reader.BaseStream.Position < _reader.BaseStream.Length)
            {
                var segment = DataSegment.Read(_reader);
                _segments.Add(segment);
            }
            
            foreach (var segment in _segments)
            {
                ReadSegment(segment);
            }
        }

        public DataSegment[] GetSegments() => _segments.ToArray();

        public List<ENetPacket> GetENetPackets()
        {
            var pkts = new List<ENetPacket>();
            foreach (var section in Sections)
            {
                if (section is GameDataSection gds)
                {
                    pkts.AddRange(gds.Chunk.ENetPackets);
                }
            }
            return pkts;
        }
        
        public List<Chunk> GetChunks()
        {
            var chunks = new List<Chunk>();
            foreach (var section in Sections)
            {
                if (section is GameDataSection gds)
                {
                    chunks.Add(gds.Chunk);}
            }
            return chunks;
        }
        
        //

        private List<ENetPacket> ReadGameDataChunk(BinaryReader reader)
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
                HandleBinaryPacket(data);
            }
        }

        protected override void HandleBinaryPacket(byte[] data)
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
                pkts = ReadGameDataChunk(reader);
            }

            if (CurrentSection is GameDataSection section)
            { 
                section.Chunk.ENetPackets.AddRange(pkts);
            }
        }
    }