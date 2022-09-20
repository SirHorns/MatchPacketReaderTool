using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive;
using System.Reactive.Linq;
using System.Windows.Input;
using MatchPacketReaderTool.Models;
using MatchPacketReaderTool.Services;
using ReactiveUI;

namespace MatchPacketReaderTool.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        public string _path = "";
        
        public string Path
        {
            get => _path;
            set => this.RaiseAndSetIfChanged(ref _path, value);
        }

        public ObservableCollection<PacketTypeViewModel> Packets { get; } = new();
        public ObservableCollection<string> pTypes { get; } = new();
        
        public ObservableCollection<string> MatchPackets { get; } = new();

        private List<Person> People { get; }

        public MainWindowViewModel()
        {
            People = new List<Person>();
            
            People.Add(new Person("Iam", "Therefore"));
            People.Add(new Person("Iam", "Therefore"));
            People.Add(new Person("Iam", "Therefore"));
            People.Add(new Person("Iam", "Therefore"));
            
            LoadPacketTypes();
            ShowOpenFileDialog = new Interaction<Unit, string?>();

            OpenFileCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var _result = await ShowOpenFileDialog.Handle(default);
                Path = _result;
                LoadMatch(Path);
            });

        }

        public ICommand OpenFileCommand { get; }
        
        public Interaction<Unit,string> ShowOpenFileDialog { get; }


        private void LoadMatch(string path)
        {
            var pacs = new MatchFile(path);
        }

        private void LoadPacketTypes()
        {
            var packets =  PacketTypes.Filters();

            foreach (var packet in packets)
            {
                var vm = new PacketTypeViewModel(packet);
                Packets.Add(vm);
                pTypes.Add(packet);
            }
        }
    }
}