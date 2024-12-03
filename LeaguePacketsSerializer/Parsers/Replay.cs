using System.Collections.Generic;
using LeaguePacketsSerializer.Packets;
using LeaguePacketsSerializer.Parsers.ChunkParsers;

namespace LeaguePacketsSerializer.Parsers;

public class Replay
{
    private List<SerializedPacket> _serialized = new();
    private List<BadPacket> _soft = new();
    private List<BadPacket> _hard = new();
    
    public BasicHeader BasicHeader { get; internal set; }
    public MetaData MetaData { get; internal set; }
    public List<Section> Sections { get; internal set; }
    public List<Chunk> Chunks { get; internal init; }
    public List<ENetPacket> RawPackets { get; internal init; }
    
    public ReplayInfo Info { get; internal set; }

    
    public List<SerializedPacket> SerializedPackets
    {
        get
        {
            if (_serialized.Count > 0)
            {
                return _serialized;
            }
            _serialized = GetSerializedPackets();
            return _serialized;
        }
    }
    public List<BadPacket> SoftBadPackets
    {
        get
        {
            if (_soft.Count > 0)
            {
                return _soft;
            }
            _soft = GetSoftBadPackets();
            return _soft;
        }
    }
    
    public List<BadPacket> HardBadPackets
    {
        get
        {
            if (_hard.Count > 0)
            {
                return _hard;
            }
            _hard = GetHardBadPackets();
            return _hard;
        }
    }
    
    private List<SerializedPacket> GetSerializedPackets()
    {
        var list = new List<SerializedPacket>();
        foreach (var chunk in Chunks)
        {
            list.AddRange(chunk.SerializedPackets);
        }
        return list;
    }

    private List<BadPacket> GetSoftBadPackets()
    {
        var list = new List<BadPacket>();
        foreach (var chunk in Chunks)
        {
            list.AddRange(chunk.SoftBadPackets);
        }
        return list;
    }

    private List<BadPacket> GetHardBadPackets()
    {
        var list = new List<BadPacket>();
        foreach (var chunk in Chunks)
        {
            list.AddRange(chunk.HardBadPackets);
        }
        return list;
    }
}