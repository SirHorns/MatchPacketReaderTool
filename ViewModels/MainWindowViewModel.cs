using System.Collections.Generic;
using System.Collections.ObjectModel;
using Avalonia.Controls.Selection;
using MatchPacketReaderTool.Models;

namespace MatchPacketReaderTool.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string Greeting => "Welcome to Avalonia!";
        public ObservableCollection<PacketTypeViewModel> Packets { get; } = new();

        public MainWindowViewModel()
        {
            LoadPacketTypes();
        }

        private async void LoadPacketTypes()
        {
            var packets = await PacketTypes.Filters();

            foreach (var packet in packets)
            {
                var vm = new PacketTypeViewModel(packet);
                Packets.Add(vm);
            }
        }
    }
}