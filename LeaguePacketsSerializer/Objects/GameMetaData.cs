using System.Collections.Generic;

namespace LeaguePacketsSerializer.Objects;

public class GameMetaData
{
    public GameKey GameKey { get; set; }
    public string GameServerAddress { get; set; }
    public int Port { get; set; }
    public string EncryptionKey { get; set; }
    public int ChunkTimeInterval { get; set; }
    public string StartTime { get; set; }
    public bool GameEnded { get; set; }
    public int LastChunkId { get; set; }
    public int LastKeyFrameId { get; set; }
    public int EndStartupChunkId { get; set; }
    public int DelayTime { get; set; }
    public List<PendingAvailableChunkInfo> PendingAvailableChunkInfo { get; set; }
    public List<object> PendingAvailableKeyFrameInfo { get; set; }
    public int KeyFrameTimeInterval { get; set; }
    public string DecodedEncryptionKey { get; set; }
    public int StartGameChunkId { get; set; }
    public int GameLength { get; set; }
    public int ClientAddedLag { get; set; }
    public bool ClientBackFetchingEnabled { get; set; }
    public int ClientBackFetchingFreq { get; set; }
    public int InterestScore { get; set; }
    public bool FeaturedGame { get; set; }
    public string CreateTime { get; set; }
    public int EndGameChunkId { get; set; }
    public int EndGameKeyFrameId { get; set; }
}