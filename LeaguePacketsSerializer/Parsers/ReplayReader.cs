﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LeaguePacketsSerializer.Parsers.ChunkParsers;
using Newtonsoft.Json;

namespace LeaguePacketsSerializer.Parsers
{
    public class ReplayReader
    {
        public Replay Replay { get; private set; }

        public Replay Read(Stream stream, ENetLeagueVersion? enetLeagueVersion)
        {
            using var reader = new BinaryReader(stream);
            
            // Basic header
            var bh = BasicHeader.Read(reader);
            
            

            if(bh.Unused == 'n' && bh.Version == 'f' && bh.Compressed == 'o' && bh.Reserved == '\0')
            {
                Replay = Nfo(reader, bh);
                return Replay;
            } 
            
            Replay = new Replay()
            {
                BasicHeader = bh
            };
            
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
            IChunkParser chunkParser;
            if(Replay.MetaData.SpectatorMode)
            {
                Console.WriteLine("[Spectator Mode Replay]");
                chunkParser = new ChunkParserSpectator(Replay.MetaData.EncryptionKey, Replay.MetaData.MatchId);
            }
            else
            {     
                Console.WriteLine("[ENet Mode Replay]");
                chunkParser = new ChunkParserENet(eNetLeagueVersion, Replay.MetaData.EncryptionKey);
            }
            chunkParser.Parse(data);
            Replay.Chunks = chunkParser.Chunks;
            Replay.RawPackets = chunkParser.Packets;
            return Replay;
        }

        

        private static Replay Nfo(BinaryReader reader, BasicHeader bh)
        {
            var replay = new Replay
            {
                RawPackets = new List<ENetPacket>(),
                BasicHeader = bh
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