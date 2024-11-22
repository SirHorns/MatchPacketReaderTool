using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;

namespace LeaguePacketsSerializer.Parsers.ChunkParsers;

public class ChunkParserSpectator : HttpProtocolHandler, IChunkParser
    {
        private BlowFish _blowfish;

        public List<Chunk> Chunks => _chunks;
        public List<ENetPacket> Packets { get; } =  new();

        public ChunkParserSpectator(byte[] key, int matchID)
        {
            var keyBlowfish = new BlowFish(Encoding.ASCII.GetBytes(matchID.ToString()));

            _blowfish = new BlowFish(keyBlowfish.Decrypt(key).Take(16).ToArray());
        }
        
        public void Parse(byte[] data)
        {
            List<DataSegment> dataSegments = new List<DataSegment>();
            // Read "chunks" from stream and hand them over to parser
            using var chunksReader = new BinaryReader(new MemoryStream(data));
            while (chunksReader.BaseStream.Position < chunksReader.BaseStream.Length)
            {
                dataSegments.Add(DataSegment.Read(chunksReader));
            }
            
            foreach (var ds in dataSegments)
            {
                Read(ds);
            }
        }

        private void ReadSpectatorChunks(BinaryReader reader)
        {
            var time = 0.0f;
            byte packetType = 0;
            var blockParam = 0;
            
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
                AddPacket(packetType, channel, blockParam, packetData, time);
            }
        }
        
        private void AddPacket(byte packetType, byte channel, int blockParam, byte[] data, float time)
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
            _currentChunk.ENetPackets.Add(pkt);
        }


        protected override void HandleBinaryPacket(byte[] data, float timeHttp)
        {
            var decrypted = _blowfish.Decrypt(data);
            using var decompressed = new MemoryStream();
            using (var compressed = new GZipStream(new MemoryStream(decrypted), CompressionMode.Decompress))
            {
                compressed.CopyTo(decompressed);
            }
            decompressed.Seek(0, SeekOrigin.Begin);
            using (var reader = new BinaryReader(decompressed))
            {
                ReadSpectatorChunks(reader);
            }
        }
    }