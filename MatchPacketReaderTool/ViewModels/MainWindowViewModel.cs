using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
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

        private readonly PacketDataManager _pdm;

        public ObservableCollection<PacketTypeViewModel> Packets { get; } = new();
        public ObservableCollection<string> pTypes { get; } = new();
        
        public ObservableCollection<string> MatchPackets { get; } = new();

        private bool _loaded = false;
        private bool IsFileLoaded
        {
            get => _loaded;
            set => this.RaiseAndSetIfChanged(ref _loaded, value);
        }

        public ObservableCollection<RawPacket> RawPackets { get; private set; } = new();

        public MainWindowViewModel()
        {
            _pdm = new();

            LoadPacketTypes();
            ShowOpenFileDialog = new Interaction<Unit, string>();

            OpenFileCommand = ReactiveCommand.CreateFromTask(async () =>
            {
                var result = await ShowOpenFileDialog.Handle(default);
                Path = result;
                IsFileLoaded = _pdm.LoadFile(Path);
                //LoadMatch(Path);
            });

        }

        public string Path
        {
            get => _path;
            set => _path = value ?? throw new ArgumentNullException(nameof(value));
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
        

        public void RaisePropertyChanged(PropertyChangedEventArgs args)
        {
            Debug.WriteLine("Here event");
            foreach (var p in _pdm.GetRawPackets())
            {
                RawPackets.Add(p);
            }
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
    }
}