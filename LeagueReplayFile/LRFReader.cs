using System.Text;
using LeagueReplayFile.Enums;
using LeagueReplayFile.Models;
using Newtonsoft.Json;

namespace LeagueReplayFile;

/// <summary>
/// Reads LRF file contents into a C# Object
/// </summary>
public class LRFReader: IDisposable
{
    public LRF LRF { get; private set; }
    public Stream Stream { get; private set; }
    public BinaryReader? BinaryReader { get; private set; }
    
    public LRF? Read(Stream stream)
    {
        Stream = stream;
        BinaryReader = new BinaryReader(stream);
        var header = ReadHeader();
        LRF = new LRF()
        {
            Type = LRFTypes.NAN,
            Stream = stream,
            BasicHeader = header
        };
        ReplayMetaData? metaData = null;
        var isNfo = header.Unused == 'n' && header.Version == 'f' && header.Compressed == 'o' && header.Reserved == '\0';
        if (isNfo)
        {
            LRF.Type = LRFTypes.NFO; // LRF is a NFO replay
            var nfo = Encoding.UTF8.GetString(BinaryReader.ReadExactBytes(4)) == "nfo";
        }
        var dataSize = BinaryReader.ReadUInt32();
        if (isNfo)
        {
            var pad = BinaryReader.ReadUInt64();
        }
        metaData = ReadMetaData((int)dataSize);
        LRF.MetaData = metaData;
        switch (metaData.SpectatorMode)
        {
            case true:
                LRF.Type = LRFTypes.HTTP;
                break;
            default:
                LRF.Type = LRFTypes.ENET;
                break;
        }
        
        return LRF;
    }

    public ReplayMetaData? ReadMetaData(Stream stream, out LRFTypes type)
    {
        BinaryReader = new BinaryReader(stream);
        var header = ReadHeader();

        ReplayMetaData? metaData = null;
        var isNFO = header.Unused == 'n' && header.Version == 'f' && header.Compressed == 'o' &&
                    header.Reserved == '\0';

        if (isNFO)
        {
            var nfo = Encoding.UTF8.GetString(BinaryReader.ReadExactBytes(4)) == "nfo";
        }
        var dataSize = BinaryReader.ReadUInt32();
        if (isNFO)
        {
            var pad = BinaryReader.ReadUInt64();
        }
        metaData = ReadMetaData((int)dataSize);
        
        if (metaData is null)
        {
            type = LRFTypes.NAN;
        }
        else
        {
           if (isNFO)
           {
               type = LRFTypes.NFO; // LRF is a NFO replay
           }
           else
           {
               type = metaData.SpectatorMode ? LRFTypes.HTTP : LRFTypes.ENET;
           } 
        }
        
        return metaData;
    }
    
    private BasicHeader ReadHeader()
    {
        return new BasicHeader()
        {
            Unused = BinaryReader.ReadByte(),
            Version = BinaryReader.ReadByte(),
            Compressed = BinaryReader.ReadByte(),
            Reserved = BinaryReader.ReadByte()
        };
    }

    public ReplayMetaData? ReadMetaData(int dataSize)
    {
        var bytes = BinaryReader.ReadExactBytes(dataSize);
        var json = Encoding.UTF8.GetString(bytes);
        var metadata = JsonConvert.DeserializeObject<ReplayMetaData>(json);
        return metadata;
    }
    
    

    

    public void Dispose() => BinaryReader?.Dispose();
}