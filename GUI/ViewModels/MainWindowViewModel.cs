using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GUI.Enums;
using GUI.Models;
using GUI.Services;
using GUI.Tools;
using LeaguePacketsSerializer.ENet;
using LeaguePacketsSerializer.Enums;
using LeaguePacketsSerializer.Parsers.ChunkParsers;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using ReplayAPI;

namespace GUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private ReplayViewModel _replayControlViewModel;
    [ObservableProperty] private ReplayModel _replayModel;
    
    
    [ObservableProperty] private bool _isWorking;
    [ObservableProperty] private bool _writeToFile;
    [ObservableProperty] private bool _replayLoaded;
    [ObservableProperty] private bool _apiServerRunning;
    
    private readonly ReplayHandler _replayHandler;
    private ReplayApiServer _apiServer;
    
    
    public MainWindowViewModel()
    {
        _apiServer = new ReplayApiServer();
        _replayHandler = new ReplayHandler();
        
        ReplayControlViewModel = new ReplayViewModel();
        ReplayModel = new ReplayModel();
    }

    // COMMANDS
    
    [RelayCommand]
    private async Task SelectReplayFile(CancellationToken token)
    {
        try
        {
            var filesService = App.Current?.Services?.GetService<FilesService>();
            if (filesService is null)
            {
                throw new NullReferenceException("Missing File Service instance.");
            }

            var file = await filesService.OpenReplayFileAsync(FileType.LRF);
            if (file is null)
            {
                return;
            }

            ReplayModel.FilePath = file.Path.AbsolutePath;
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }
    
    [RelayCommand]
    private async Task ParseReplay(CancellationToken token)
    {
        if (string.IsNullOrEmpty(ReplayModel.FilePath))
        {
            return;
        }
        IsWorking = true;

        await Task.Run(() =>
        {
            _replayHandler.ParseReplay(ReplayModel.FilePath, ENetLeagueVersion.Patch420, WriteToFile);
            if (_replayHandler.Replay is null)
            {
                ReplayLoaded = false;
                IsWorking = false;
                return;
            }

            ReplayModel.Replay = _replayHandler.Replay;
            UpdateReplayVm();

            ReplayService.SetReplay(_replayHandler.Replay);
            
        }, token);
        ReplayLoaded = true;
        IsWorking = false;
    }

    private void UpdateReplayVm()
    {
        var replay = _replayHandler.Replay;
        ReplayControlViewModel.Set(replay);
    }

    [RelayCommand]
    private async Task UnhashReplay(CancellationToken token)
    {
        IsWorking = true;

        await Task.Run(() =>
        {
            if (string.IsNullOrEmpty(ReplayModel.FilePath))
            {
                Console.WriteLine("No replay to unhash");
                return;
            }

            var fileName = Path.GetFileNameWithoutExtension(ReplayModel.FilePath);
            var path = $"ParsedReplay/{fileName}/SerializedPackets.json";

            if (!File.Exists(path))
            {
                Console.WriteLine($"Couldn't find serialized files: {path}");
                IsWorking = false;
                return;
            }

            Console.WriteLine("Unhashing replay...");

            var res = _replayHandler.UnhashReplay(_replayHandler.Replay.SerializedPackets);
            Write($"ParsedReplay/{fileName}/UnHhshedSerializedPackets.json", res);

            Console.WriteLine("Finished Unhashing replay!");
        }, token);
        
        IsWorking = false;
    }
    
    private void Write(string path, object obj)
    {
        using var fileStream = File.CreateText(path);
        var jsonSerializer = new JsonSerializer
        {
            Formatting = Formatting.Indented
        };
        jsonSerializer.Serialize(fileStream, obj);
    }

    [RelayCommand]
    private async Task SaveFile()
    {
        try
        {
            var filesService = App.Current?.Services?.GetService<FilesService>();
            if (filesService is null) throw new NullReferenceException("Missing File Service instance.");

            var file = await filesService.SaveFileAsync();
            if (file is null) return;


            // Limit the text file to 1MB so that the demo wont lag.
            if (ReplayModel.FilePath?.Length <= 1024 * 1024 * 1)
            {
                var stream = new MemoryStream(Encoding.Default.GetBytes((string)ReplayModel.FilePath));
                await using var writeStream = await file.OpenWriteAsync();
                await stream.CopyToAsync(writeStream);
            }
            else
            {
                throw new Exception("File exceeded 1MB limit.");
            }
        }
        catch (Exception e)
        {
        }
    }

    [RelayCommand]
    private void StartApiServer()
    {
        if (ApiServerRunning)
        {
            return;
        }
        
        ApiServerRunning = true;
        _ = Task.Run(() => _apiServer.Run());
    }
}