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
using LeaguePacketsSerializer.ReplayParser;
using Microsoft.Extensions.DependencyInjection;

namespace GUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private ReplayHandler _replayHandler = new();

    [ObservableProperty] private string? _fileText;
    [ObservableProperty] private bool _isWorking;
    
    [RelayCommand]
    private void OpenLrf()
    {
        
    }
    
    [RelayCommand]
    private void OpenPlrf()
    {
        
    }

    [RelayCommand]
    private void OpenJson()
    {
        
    }
    
    [RelayCommand]
    private async Task ParseReplay(CancellationToken token)
    {
        await Task.Run(()=> { _replayHandler.ParseReplay(FileText, ENetLeagueVersion.Patch420); }, token);
    }

    [RelayCommand]
    private async Task UnhashReplay(CancellationToken token)
    {
        await Task.Run(()=> { _replayHandler.UnhashReplay(FileText); }, token);
    }
    
     

    [RelayCommand]
    private async Task OpenLrfFile(CancellationToken token)
    {
        try
        {
            var filesService = App.Current?.Services?.GetService<FilesService>();
            if (filesService is null)
            {
                throw new NullReferenceException("Missing File Service instance.");
            }

            var file = await filesService.OpenFileAsync(FileType.LRF);
            if (file is null)
            {
                return;
            }

            FileText = file.Path.AbsolutePath;
            
            //await using var readStream = await file.OpenReadAsync();
            //using var reader = new StreamReader(readStream);
            //FileText = await reader.ReadToEndAsync(token);
        }
        catch (Exception e)
        {
            //ignore
        }
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
            if (FileText?.Length <= 1024 * 1024 * 1)
            {
                var stream = new MemoryStream(Encoding.Default.GetBytes((string)FileText));
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