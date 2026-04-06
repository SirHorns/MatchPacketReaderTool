using LeagueReplayFile.Models;
using LeagueReplayFile.Protocols.ENet;

namespace LeagueReplayFile.Parsers;

public interface ILRFParser
{
    /// <summary>
    /// 
    /// </summary>
    List<DataSegment> Segments { get; }
    /// <summary>
    /// 
    /// </summary>
    List<ENetPacket> Packets { get; }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="data"></param>
    void Read(byte[] data);
}
