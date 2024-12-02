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
using LeaguePacketsSerializer.Parsers.ChunkParsers;
using Microsoft.Extensions.DependencyInjection;

namespace GUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private ReplayInfoVM _replayInfoVm;
    [ObservableProperty] private bool _isWorking;
    [ObservableProperty] private ReplayModel _replayModel;
    private readonly ReplayHandler _replayHandler;
    
    public MainWindowViewModel()
    {
        ReplayInfoVm = new ReplayInfoVM();
        _replayHandler = new ReplayHandler();
        ReplayModel = new ReplayModel();
    }
    
    
    
    
    
    
    
    
    
    
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
            _replayHandler.ParseReplay(ReplayModel.FilePath, ENetLeagueVersion.Patch420, true);
            if (_replayHandler.Replay is null)
            {
                return;
            }

            var info = _replayHandler.Replay.Info;
            ReplayInfoVm.SetResults(info);
            ReplayModel.Replay = _replayHandler.Replay;
        }, token);
        
        IsWorking = false;
    }

    [RelayCommand]
    private async Task UnhashReplay(CancellationToken token)
    {
        if (string.IsNullOrEmpty(ReplayModel.FilePath))
        {
            return;
        }
        IsWorking = true;
        await Task.Run(() =>
        {
            _replayHandler.UnhashReplay(ReplayModel.FilePath); 
        }, token);
        IsWorking = false;
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
}