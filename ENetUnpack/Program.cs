using ENetUnpack.ReplayParser;
using Newtonsoft.Json;
using System;
using System.IO;

namespace ENetUnpack
{
    class Program
    {
        static void Main(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Bad arguments!");
                Console.Error.WriteLine("argument 1 is path to .lrf file");
                Console.Error.WriteLine("argument 2 is league modified enet version which is one of:");
                Console.Error.WriteLine("Seasson12, Seasson34, Patch420");
            }
            
            var filename = args[0];
            ENetLeagueVersion? version = (ENetLeagueVersion)Enum.Parse(typeof(ENetLeagueVersion), args[1]);
            
            if(!filename.EndsWith(".lrf"))
            {
                Console.Error.WriteLine("Filename should end with .lrf!");
            }

            var packets = Replay.ReadPackets(File.OpenRead(filename), version);
            var json = JsonConvert.SerializeObject(packets, Formatting.Indented);
            File.WriteAllText(filename.Replace(".lrf", ".rlp.json"), json);
        }
    }
}
