using MatchPacketReaderTool.Models;

namespace MatchPacketReaderTool.Services;

public class PacketDataManager
{
    private string _filePath = "";
    private string _rawJson;
    private MatchFile _matchFile;
    
    public string FilePath
    {
        get => _filePath;
        private set => _filePath = value;
    }

    public void LoadFile(string path)
    {
        FilePath = path;
    }
}