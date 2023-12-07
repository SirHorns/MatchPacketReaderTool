using System;
using System.Threading.Tasks;
using ENetUnpack.ReplayParser;
using ReplayUnhasher;

namespace GUI.Models;

public class ReplayHandler
{
    //private PacketsSerializer _serializer = new();
    private Unhasher _unhasher = new();

    public void ParseReplay(string replayPath, ENetLeagueVersion version)
    {
        try
        {
            //_serializer.Serialize(replayPath, version);
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