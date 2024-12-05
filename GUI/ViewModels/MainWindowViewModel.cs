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
using Microsoft.Extensions.DependencyInjection;
using ReplayAPI;

namespace GUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty] private ReplayViewModel _replayControlViewModel;
    [ObservableProperty] private ReplayModel _replayModel;
    [ObservableProperty] private ApiServerModel _apiModel;
    
    
    [ObservableProperty] private bool _isWorking;
    [ObservableProperty] private bool _writeToFile;
    [ObservableProperty] private bool _replayLoaded;
    [ObservableProperty] private bool _apiRunning;
    
    
    public MainWindowViewModel()
    {
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
            if (ReplayModel.Parse(WriteToFile))
            {
                ReplayLoaded = true;
            }
        }, token);

        if (ReplayLoaded)
        {
            ReplayControlViewModel.Set(ReplayModel.Replay);
        }
        
        ReplayService.SetReplay(ReplayModel.Replay);
        IsWorking = false;
    }
    
    [RelayCommand]
    private Task StartApiServer()
    {
        if (ApiModel.IsRunning)
        {
            return Task.CompletedTask;
        }
        
        ApiRunning = ApiModel.IsRunning;
        _ = Task.Run(ApiModel.Run);
        return Task.CompletedTask;
    }
    
    /*[RelayCommand]
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
            Console.WriteLine("Failed to ");
        }
    }*/
}