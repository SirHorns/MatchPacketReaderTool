using CommunityToolkit.Mvvm.ComponentModel;
using LeaguePacketsSerializer;

namespace GUI.ViewModels;

public partial class ReplayInfoVM: ViewModelBase
{
    [ObservableProperty] private ReplayResultInfo _results;
}