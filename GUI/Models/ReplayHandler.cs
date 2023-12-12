using System;
using System.IO;
using System.Threading.Tasks;
using ENetUnpack.ReplayParser;
using LeaguePacketsSerializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using ReplayUnhasher;

namespace GUI.Models;

public class ReplayHandler
{
    private ReplaySerializer _serializer = new();
    private Unhasher _unhasher = new();

    public void ParseReplay(string replayPath, ENetLeagueVersion version)
    {
        try
        {
            _serializer.Serialize(replayPath, version);
            _serializer.GenerateReplayJsons();
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            throw;
        }
    }

    public void UnhashReplay()
    {
        Task.Run(()=> _unhasher.UnhashReplay());
    }
}