using System.Collections.Generic;
using System.IO;
using LeaguePacketsSerializer.ENet;
using LeaguePacketsSerializer.Enums;
using LeaguePacketsSerializer.Packets;
using LeaguePacketsSerializer.Parsers.ChunkParsers;
using Newtonsoft.Json;

namespace LeaguePacketsSerializer.Parsers;

public class Replay
{
    public ReplayType Type { get; internal set; }
    public BasicHeader BasicHeader { get; internal set; }
    public MetaData MetaData { get; internal set; }
    public List<Section> Sections { get; internal set; } = new();
    public List<Chunk> Chunks { get; internal init; }
    public List<ENetPacket> RawPackets { get; internal init; }
    public ReplayInfo ReplayInfo { get; internal set; }
    public List<SerializedPacket> SerializedPackets { get; } = new();
    public List<BadPacket> SoftBadPackets { get; } = new();
    public List<BadPacket> HardBadPackets { get; } = new();

    internal void Update()
    {
        if (Type == ReplayType.ENET)
        {
            return;
        }

        if (Type == ReplayType.SPECTATOR)
        {
            foreach (var chunk in Chunks)
            {
                SerializedPackets.AddRange(chunk.SerializedPackets);
                SoftBadPackets.AddRange(chunk.SoftBadPackets);
                HardBadPackets.AddRange(chunk.HardBadPackets);
            }
        }
    }

    protected record General(ReplayType Type, BasicHeader BasicHeader, ReplayInfo ReplayInfo);
    
    public void WriteToJsons(string basePath = "/replay")
    {
        var path = $"{basePath}/General.json";
        var meta = new General(Type, BasicHeader, ReplayInfo);
        Write(path, meta);
        
        path = $"{basePath}/MetaData.json";
        Write(path, MetaData);
        
        path = $"{basePath}/Sections.json";
        Write(path, Sections);
        
        path = $"{basePath}/Chunks.json";
        Write(path, Chunks);
        
        path = $"{basePath}/RawPackets.json";
        Write(path, RawPackets);
        
        path = $"{basePath}/SerializedPackets.json";
        Write(path, SerializedPackets);
        
        path = $"{basePath}/SoftBadPackets.json";
        Write(path, SoftBadPackets);
        
        path = $"{basePath}/HardBadPackets.json";
        Write(path, HardBadPackets);
    }

    private void Write(string path, object obj)
    {
        using var fileStream = File.CreateText(path);
        var jsonSerializer = new JsonSerializer
        {
            Formatting = Formatting.Indented
        };
        jsonSerializer.Serialize(fileStream, obj);
    }
}