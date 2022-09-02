using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls.Selection;

namespace MatchPacketReaderTool.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";
        public ObservableCollection<ItemViewModel> MyItems { get; } = new();

        public MainWindowViewModel()
        {
            MyItems.Add(new ItemViewModel());
            MyItems.Add(new ItemViewModel());
            MyItems.Add(new ItemViewModel());
            MyItems.Add(new ItemViewModel());
            MyItems.Add(new ItemViewModel());
            MyItems.Add(new ItemViewModel());
            MyItems.Add(new ItemViewModel());
            MyItems.Add(new ItemViewModel());
            MyItems.Add(new ItemViewModel());
        }
    }
}