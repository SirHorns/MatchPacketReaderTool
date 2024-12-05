using System;
using System.IO;
using GUI.Services;
using GUI.Tools;
using LeaguePacketsSerializer;
using LeaguePacketsSerializer.ENet;
using LeaguePacketsSerializer.Parsers;
using Newtonsoft.Json;

namespace GUI.Models;

public class ReplayModel
{
    public Replay Replay { get; set; }
    public string FilePath { get; set; }

    public ReplayInfo Info => Replay.ReplayInfo;
    
    private readonly ReplayHandler _replayHandler = new ();

    public bool Parse(bool writeToFile)
    {
        if (string.IsNullOrEmpty(FilePath))
        {
            Console.WriteLine("No replay was given!");
            return false;
        }
            
        _replayHandler.ParseReplay(FilePath, ENetLeagueVersion.Patch420, writeToFile);
        if (_replayHandler.Replay is null)
        {
            return  false;
        }
        
        //TODO: migrate unhashing into serilizer
        /*if (_writeToFile)
        {
            var fileName = Path.GetFileNameWithoutExtension(FilePath);
            var path = $"ParsedReplay/{fileName}/SerializedPackets.json";

            if (!File.Exists(path))
            {
                Console.WriteLine($"Couldn't find serialized files: {path}");
                IsWorking = false;
                return  false;
            }

            Console.WriteLine("Unhashing replay...");

            var res = _replayHandler.UnhashReplay(_replayHandler.Replay.SerializedPackets);
            Write($"ParsedReplay/{fileName}/UnHhshedSerializedPackets.json", res);

            Console.WriteLine("Finished Unhashing replay!");
        }*/
            
        Replay = _replayHandler.Replay;
        return true;
    }
    
    private void WriteReplayToFile(string path, object obj)
    {
        using var fileStream = File.CreateText(path);
        var jsonSerializer = new JsonSerializer
        {
            Formatting = Formatting.Indented
        };
        jsonSerializer.Serialize(fileStream, obj);
    } 
}