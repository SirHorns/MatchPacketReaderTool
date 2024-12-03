using System.IO;
using LeaguePacketsSerializer.Parsers;

namespace LeaguePacketsSerializer.ENet;

public class ENetProtocolSendFragment : ENetProtocol
{
    public ushort StartSequenceNumber { get; set; }
    public uint FragmentCount { get; set; }
    public uint FragmentNumber { get; set; }
    public uint TotalLength { get; set; }
    public uint FragmentOffset { get; set; }
    public byte[] Data { get; set; }

    public ENetProtocolSendFragment(ENetProtocolHeader protocolHeader, ENetProtocolCommandHeader protocolCommandHeader, BinaryReader reader)
    {
        StartSequenceNumber = reader.ReadUInt16(true);
        ushort dataLength = reader.ReadUInt16(true);
        FragmentCount = reader.ReadUInt32(true);
        FragmentNumber = reader.ReadUInt32(true);
        TotalLength = reader.ReadUInt32(true);
        FragmentOffset = reader.ReadUInt32(true);
        Data = reader.ReadExactBytes(dataLength);
    }
}