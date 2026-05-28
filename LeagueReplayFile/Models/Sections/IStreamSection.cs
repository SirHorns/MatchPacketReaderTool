using System.IO.Compression;

namespace LeagueReplayFile.Models.Sections;

public interface IStreamSection
{
    byte[] PacketData { get; }
}