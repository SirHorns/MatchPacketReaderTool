using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using LeaguePacketsSerializer;
using LeaguePacketsSerializer.Enums;
using LeaguePacketsSerializer.Packets;
using LeaguePacketsSerializer.Parsers;

namespace GUI.ViewModels;

public partial class ReplayViewModel: ViewModelBase
{
    [ObservableProperty] private ReplayInfo _replayInfo;
    [ObservableProperty] private List<Chunk> _chunkList;
    [ObservableProperty] private List<SerializedPacket> _packetList;
    [ObservableProperty] private Replay _replay;
    [ObservableProperty] private bool _chunksLoaded;
    
    public ReplayViewModel()
    {
        _replayInfo = new ReplayInfo(0, 0, 0, 0, 0, "", "");
        _chunkList = [];
        _packetList = [];

        ChunksLoaded = true;
        for (var i = 1; i < 101; i++)
        {
            _chunkList.Add(new Chunk()
            {
                ID = i,
                ENetPackets = [],
                SerializedPackets = [],
                SoftBadPackets = [],
                HardBadPackets = [],
            });
        }
        
        for (var i = 1; i < 101; i++)
        {
            _packetList.Add(new SerializedPacket()
            {
                Type = "BADWOLF"
            });
        }
    }
    
    public void Set(Replay replay)
    {
        Replay = replay;
        ReplayInfo = replay.ReplayInfo;
        PacketList = [];
        
        if (replay.Type == ReplayType.SPECTATOR)
        {
            var chunks = _replay.Chunks;
            ChunksLoaded = true;
            ChunkList = chunks;
            
            foreach (var chunk in chunks)
            {
                PacketList.AddRange(chunk.SerializedPackets);
            }
        }
        else
        {
            PacketList.AddRange(Replay.SerializedPackets);
        }
    }
}