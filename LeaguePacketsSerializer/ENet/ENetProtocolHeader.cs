using System;
using System.Collections.Generic;
using System.IO;
using LeaguePacketsSerializer.Parsers;

namespace LeaguePacketsSerializer.ENet;

public class ENetProtocolHeader
{
    public uint? CheckSum { get; set; } = null;
    public uint SessionID { get; set; }
    public ushort? PeerID { get; set; } = null;
    public ushort? TimeSent { get; set; } = null;
    public float TimeRecieved { get; set; }
    public ENetLeagueVersion ENetLeagueVersion { get; set; }

    public static readonly Dictionary<ENetLeagueVersion, int> ProtocolHeaderSizes = new Dictionary<ENetLeagueVersion, int>
    {
        [ENetLeagueVersion.Seasson12] = 8,
        [ENetLeagueVersion.Seasson34] = 4,
        [ENetLeagueVersion.Patch420] = 8,
    };

    public ENetProtocolHeader(BinaryReader reader, float timeRecieved, ENetLeagueVersion enetLeagueVersion)
    {
        switch (enetLeagueVersion)
        {
            case ENetLeagueVersion.Seasson12:
            {
                SessionID = reader.ReadUInt32(true);
                ushort peerID = reader.ReadUInt16(true);
                if((peerID & 0x7FFF) != 0x7FFF)
                {
                    PeerID = peerID;
                }
                if ((peerID & 0x8000) > 0)
                {
                    TimeSent = reader.ReadUInt16();
                }
            }
                break;
            case ENetLeagueVersion.Seasson34:
            {
                SessionID = reader.ReadByte();
                byte peerID = reader.ReadByte();
                if ((peerID & 0x7F) != 0x7F)
                {
                    PeerID = peerID;
                }
                if ((peerID & 0x80) > 0)
                {
                    TimeSent = reader.ReadUInt16();
                }
            }
                break;
            case ENetLeagueVersion.Patch420:
            {
                CheckSum = reader.ReadUInt32(true);
                SessionID = reader.ReadByte();
                byte peerID = reader.ReadByte();
                if ((peerID & 0x7F) != 0x7F)
                {
                    PeerID = peerID;
                }
                if ((peerID & 0x80) > 0)
                {
                    TimeSent = reader.ReadUInt16();
                }
            }
                break;
            default:
                throw new NotImplementedException();
        }
        TimeRecieved = timeRecieved;
        ENetLeagueVersion = enetLeagueVersion;
    }
}