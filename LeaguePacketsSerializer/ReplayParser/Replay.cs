using System.Collections.Generic;
using LeaguePacketsSerializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace ENetUnpack.ReplayParser;

public class Replay
{
    [JsonProperty("metadata")]
    public MetaData MetaData { get; set; }

    private IChunkParser ChunkParser { get; set; }

    public List<ENetPacket> Packets { get; set; }

    public void SetChunkParser(bool isSpectator)
    {
        
    }
}