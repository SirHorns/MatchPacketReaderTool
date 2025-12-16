using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using LeaguePackets;
using LeaguePackets.Game;
using LeaguePacketsSerializer.ENet;
using LeaguePacketsSerializer.Enums;
using LeaguePacketsSerializer.GameServer.Enums;
using LeaguePacketsSerializer.Packets;
using LeaguePacketsSerializer.Parsers;
using LeaguePacketsSerializer.Parsers.ChunkParsers;
using LeaguePacketsSerializer.Readers;
using LeaguePacketsSerializer.Replication;
using Newtonsoft.Json;
using ENetPacket = LeaguePacketsSerializer.ENet.ENetPacket;

namespace LeaguePacketsSerializer;

public class ReplaySerializer
{
    private ENetLeagueVersion _version;

    private ReplayReader _replayReader;

    public ReplaySerializer() { }

    public Replay Serialize(Stream stream, ENetLeagueVersion version = ENetLeagueVersion.Patch420)
    {
        _replayReader = new ReplayReader();
        _version = version;
        _replayReader.Read(stream, _version);
        _replayReader.ConstructReplay();
        var replay = _replayReader.GetReplay();
        PacketsSerializer.ParsePackets(ref replay);
        _replayReader = null;
        return replay;
    }

    public void SerializeToFile(Replay replay, string filePath)
    {
        Console.WriteLine("Writing Replay to json file...");

        var fileName = Path.GetFileNameWithoutExtension(filePath);
        Directory.CreateDirectory($"ParsedReplay//{fileName}");
        
        replay.WriteToJsons($"ParsedReplay//{fileName}");
        
        return;
        Directory.CreateDirectory("ParsedReplay");
        var path = $"ParsedReplay//{Path.GetFileNameWithoutExtension(filePath)}.json";
        
        using var fileStream = File.CreateText(path);
        var jsonSerializer = new JsonSerializer
        {
            Formatting = Formatting.Indented
        };
        jsonSerializer.Serialize(fileStream, replay);
    }
}