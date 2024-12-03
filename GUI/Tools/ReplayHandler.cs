using System;
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
            var res= _serializer.Serialize(replayPath, version, writeToFile);
            _packetArray = new string[res.SerializedPackets.Count];
            //var ja = JsonConvert.SerializeObject(res.SerializedPackets);
            //JPackets = JArray.Parse(ja) ;
            UnhashReplay(res.SerializedPackets);
            Replay = res;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            _serializer = new ReplaySerializer();
        }
    }

    public void UnhashReplay()
    {
        Task.Run(() =>
        {
            
        });
    }
    
    public void UnhashReplay(string filePath)
    {
        Task.Run(()=> _unhasher.UnhashReplay());
    }
    
    public void UnhashReplay(List<SerializedPacket> packets)
    {
        Task.Run(() =>
        {
            var total = packets.Count;
            var chunks = Math.Round(total * 0.05f, 0);
            double trigger = 0;
            double mark = chunks;
            List<string> pktsjson = new();
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
        });
        return;
    }
}