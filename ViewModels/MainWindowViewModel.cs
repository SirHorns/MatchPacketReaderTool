using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Windows.Input;
using MatchPacketReaderTool.Models;
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
            ShowDiag = new Interaction<string, string>();

            OpenFileCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var _result = await ShowDiag.Handle("");
                Path = _result;
                LoadMatch(Path);
            });

        }

        public ICommand OpenFileCommand { get; }
        
        public Interaction<string,string> ShowDiag { get; }


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