using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeaguePackets;
using LeaguePackets.Game;
using LeaguePacketsSerializer.GameServer.Enums;
using LeaguePacketsSerializer.Packets;
using LeaguePacketsSerializer.Parsers;
using LeaguePacketsSerializer.Parsers.ChunkParsers;
using LeaguePacketsSerializer.Replication;
using Newtonsoft.Json;
using ENetPacket = LeaguePacketsSerializer.Parsers.ChunkParsers.ENetPacket;

namespace LeaguePacketsSerializer;

public record ReplayInfo(int Chunks, int Good, int Soft, int Hard, string SoftBadIds, string HardBadIds);

public class ReplaySerializer
{
    private string _filePath;
    private ENetLeagueVersion _version;

    private ReplayReader _replayReader = new();
    private PacketsSerializer _packetsSerializer;
    private Replay _replay { get; set; }

    public ReplaySerializer()
    {
        _packetsSerializer = new PacketsSerializer();
        DataDict.Initialize();
    }

    public Replay Serialize(string filePath, ENetLeagueVersion version = ENetLeagueVersion.Patch420, bool writeToFile = false)
    {
        _filePath = filePath;
        _version = version;

        Console.WriteLine("Reading replay file...");
        
        _replayReader.Read(File.OpenRead(_filePath), _version);
        _replayReader.ConstructReplay();
        _replay = _replayReader.GetReplay();
        
        _packetsSerializer.ParsePackets(_replay);
        
        _replayReader = null;
        _replay.Info = PrintResults(_replay);
        
        if (writeToFile)
        {
            //SerializeToFile(_replay);
        }
        
        Console.WriteLine("Finished reading replay file!");
        
        return _replay;
    }

    private void SerializeToFile(Replay replay)
    {
        Console.WriteLine("Writing Replay to json file...");
        Directory.CreateDirectory("ParsedReplay");
        var path = $"ParsedReplay//{Path.GetFileNameWithoutExtension(_filePath)}.json";
        
        using var fileStream = File.CreateText(path);
        var jsonSerializer = new JsonSerializer
        {
            Formatting = Formatting.Indented
        };
        jsonSerializer.Serialize(fileStream, replay);
    }
    
    private ReplayInfo PrintResults(Replay replay)
    {
        var info = new ReplayInfo(
            replay.Chunks.Count, 
            replay.SerializedPackets.Count, 
            replay.SoftBadPackets.Count,
            replay.HardBadPackets.Count,
            string.Join(",", replay.SoftBadPackets.Select(x => x.RawID.ToString()).Distinct()),
            string.Join(",", replay.HardBadPackets.Select(x => x.RawID.ToString()).Distinct()));
        
        Console.WriteLine("[Processed]");
        Console.WriteLine($"- Chunks: {info.Chunks}");
        Console.WriteLine($"===Packets===");
        Console.WriteLine($"- Good: {info.Good}");
        Console.WriteLine($"- Soft: {info.Soft}");
        Console.WriteLine($"- Hard: {info.Hard}");
        Console.WriteLine($"Soft bad IDs:{info.SoftBadIds}");
        Console.WriteLine($"Hard bad IDs:{info.HardBadIds}");
        
        return info;
    }
}