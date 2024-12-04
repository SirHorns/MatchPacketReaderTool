using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using LeaguePacketsSerializer.ENet;
using LeaguePacketsSerializer.Enums;
using LeaguePacketsSerializer.Parsers.ChunkParsers;

namespace LeaguePacketsSerializer.Parsers
{
    public class ReplayReader
    {
        private Replay _replay;
        private bool _constructed = false;
        
        public ReplayType Type { get; private set; } = ReplayType.NAN;

        public BasicHeader BasicHeader { get; internal set; }
        public MetaData MetaData { get; private set; }
        public List<ENetPacket> RawPackets { get; private set; }
        public List<Chunk> Chunks { get; private set; }


        public IChunkParser ChunkParser { get; private set; }
        

        public void Read(Stream stream, ENetLeagueVersion? enetLeagueVersion)
        {
            _constructed = false;
            _replay = null;
            
            using var reader = new BinaryReader(stream);
            BasicHeader = BasicHeader.Read(reader);
            
            if(BasicHeader.Unused == 'n' && BasicHeader.Version == 'f' && BasicHeader.Compressed == 'o' && BasicHeader.Reserved == '\0')
            {
                Type = ReplayType.NFO;
                Nfo(reader);
                return;
            } 
            
            MetaData = MetaData.Read(reader);
            
            //TODO: Extend Replay return it?
            
            // Binary data offset start position
            var offsetStart = stream.Position;

            // Stream data
            var dataOffset = MetaData.DataIndex.First(kvp => kvp.Key == "stream").Value;
            var data = reader.ReadExactBytes(dataOffset.Size);

            if((data[0] & 0x4C) != 0)
            {
                data = BDODecompress.Decompress(data);
            }
            
            

            // Type of parser spectator or in-game/ENet
            if(MetaData.SpectatorMode)
            {
                Spectator(data);
            }
            else
            {     
                // FIXME: detect correct league version
                // TODO: determining where exact breaking changes in league ENet are??
                ENet(data, enetLeagueVersion ?? ENetLeagueVersion.Seasson12);
            }
            
            RawPackets = ChunkParser.Packets;
        }

        public void ConstructReplay()
        {
            if (_constructed)
            {
                return;
            }
            
            _replay = new Replay()
            {
                Type = Type,
                BasicHeader = BasicHeader,
                MetaData = MetaData,
                RawPackets = RawPackets,
                Chunks = Chunks ?? new List<Chunk>(),
            };

            if (ChunkParser is SpectatorChunkParser spectator)
            {
                _replay.Sections = spectator.Sections;
            }
            
            _constructed = false;
        }
        
        public Replay GetReplay()
        {
            return _replay;
        }
        
        //
        
        private void Nfo(BinaryReader reader)
        {
            RawPackets = new List<ENetPacket>();
            var first = true;
            while(reader.BaseStream.Position < reader.BaseStream.Length)
            {
                var nfo = first || Encoding.UTF8.GetString(reader.ReadExactBytes(4)) == "nfo";
                first = false;

                var dataSize = (int)reader.ReadUInt32();
                if(nfo)
                {
                    MetaData = MetaData.ReadNFO(reader, dataSize); 
                    //JObject.Parse(jsonString); //TODO: Extend Replay return it?
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

                    RawPackets.Add(new ENetPacket()
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
        }

        private void Spectator(byte[] data)
        {
            Console.WriteLine("[Spectator Mode Replay]");
            
            Type = ReplayType.SPECTATOR;
            var parser = new SpectatorChunkParser(MetaData.EncryptionKey, MetaData.MatchId);
            ChunkParser = parser;
            parser.Read(data);
            Chunks = parser.GetChunks();
        }

        private void ENet(byte[] data, ENetLeagueVersion enetLeagueVersion)
        {
            Console.WriteLine("[ENet Mode Replay]");
            
            Type = ReplayType.ENET;
            var parser = new ENetChunkParser(enetLeagueVersion, MetaData.EncryptionKey);
            ChunkParser = parser;
            parser.Read(data);
        }
    }
}
