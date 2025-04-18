﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using LeaguePacketsSerializer;
using LeaguePacketsSerializer.ENet;
using LeaguePacketsSerializer.Packets;
using LeaguePacketsSerializer.Parsers;
using LeaguePacketsSerializer.Parsers.ChunkParsers;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReplayUnhasher;

namespace GUI.Tools;

public class ReplayHandler
{
    private ReplaySerializer _serializer = new();
    private Unhasher _unhasher = new();
    private string[] _packetArray;
    public JArray JPackets { get; private set; }
    public Replay? Replay { get; private set; }

    public void ParseReplay(string replayPath, ENetLeagueVersion version, bool writeToFile = false)
    {
        if (string.IsNullOrEmpty(replayPath))
        {
            return;
        }
        
        try
        {
            Replay = _serializer.Serialize(replayPath, version, writeToFile);
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _serializer = new ReplaySerializer();
            throw;
        }
    }
    
    public List<string> UnhashReplay(List<SerializedPacket> packets)
    {
        var total = packets.Count;
        var chunks = Math.Round(total * 0.05f, 0);
        double trigger = 0;
        var mark = chunks;
        List<string> pktsjson = [];
            
        Parallel.ForEach(packets, pkt =>
        {
            var p = JsonConvert.SerializeObject(pkt);
            pktsjson.Add(_unhasher.Unhash(p));
            if (trigger >= mark)
            {
                //Console.WriteLine($"{trigger}/{total}");
                mark += chunks;
            }

            trigger++;
        });

        return pktsjson;
    }
}