using System.Collections.Generic;
using CommunityToolkit.Mvvm.ComponentModel;
using LeaguePacketsSerializer;
using LeaguePacketsSerializer.Parsers;

namespace GUI.ViewModels;

public partial class ReplayInfoVM: ViewModelBase
{
    [ObservableProperty] private ReplayInfo _results;
    [ObservableProperty] private List<Chunk> _chunkList;

    public ReplayInfoVM()
    {
        _chunkList = [];
        for (var i = 0; i < 64; i++)
        {
            _chunkList.Add(new Chunk()
            {
                ID = 0,
                ENetPackets = [],
                SerializedPackets = [],
                SoftBadPackets = [],
                HardBadPackets = []
            });
        }
    }
    
    public void SetResults(ReplayInfo results)
    {
        Results = results;
    }
}