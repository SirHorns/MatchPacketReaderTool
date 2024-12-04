using System;
using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using GUI.Controls;
using LeaguePacketsSerializer;
using LeaguePacketsSerializer.ENet;
using LeaguePacketsSerializer.Enums;
using LeaguePacketsSerializer.Packets;
using LeaguePacketsSerializer.Parsers;

namespace GUI.ViewModels;

public partial class ReplayViewModel: ViewModelBase
{
    [ObservableProperty] private ReplayInfo _replayInfo;
    [ObservableProperty] private List<Chunk> _chunkList;
    [ObservableProperty] private Replay _replay;
    [ObservableProperty] private bool _chunksLoaded;
    
    public ReplayViewModel()
    {
        _replayInfo = new ReplayInfo(0, 0, 0, 0, 0, "", "");
        _chunkList = [];

        return;
        
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
    }
    
    public void Set(Replay replay)
    {
        Replay = replay;
        ReplayInfo = replay.ReplayInfo;
        
        if (replay.Type == ReplayType.SPECTATOR)
        {
            ChunksLoaded = true;
            ChunkList = _replay.Chunks;
        }
    }
}