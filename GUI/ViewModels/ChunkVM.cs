using CommunityToolkit.Mvvm.ComponentModel;
using LeaguePacketsSerializer.Parsers;

namespace GUI.ViewModels;

public partial class ChunkVM: ViewModelBase
{
    [ObservableProperty] private Chunk _selectedChunk;
}