
using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using LeaguePackets.Game.Common;

namespace LeaguePackets.Game
{
    //NOTE: content varies on gamemode so its up to consumer of packet how to process/read it
    public class S2C_HeroStats : GamePacket // 0x46
    {
        public override GamePacketID ID => GamePacketID.S2C_HeroStats;
        public byte[] Data { get; set; } = new byte[0];

        protected override void ReadBody(ByteReader reader)
        {

            int size = reader.ReadInt32();
            this.Data = reader.ReadBytes(size - 4);
        }
        protected override void WriteBody(ByteWriter writer)
        {
            writer.WriteInt32(Data.Length + 4);
            writer.WriteBytes(Data);
        }
    }
}
