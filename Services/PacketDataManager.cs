using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using MatchPacketReaderTool.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace MatchPacketReaderTool.Services;

public class PacketDataManager
{
    private string _filePath = "";
    private string _rawJson;
    private MatchFile _matchFile;
    
    private readonly JsonSerializer _serializer = new();
    
    public string FilePath
    {
        get => _filePath;
        private set => _filePath = value;
    }

    public bool LoadFile(string path)
    {
        FilePath = path;
        
        try
        { 
            _matchFile = new MatchFile(FilePath);
            return true;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
            return false;
        }
    }

    public List<RawPacket> GetRawPackets()
    {
        List<RawPacket> p = new();
        
        foreach (var raw in _matchFile.MatchJson)
        {
            using StringReader sr = new StringReader(raw);
            using JsonReader reader = new JsonTextReader(sr);
            
            try
            {
                var rawp = _serializer.Deserialize<RawPacket>(reader);
                if (rawp != null) p.Add(rawp);
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
        }

        return p;
    }
}