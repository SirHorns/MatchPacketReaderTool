using System;
using System.IO;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ENetUnpack.ReplayParser;
using GUI.Enums;
using GUI.Models;
using GUI.Services;
using Microsoft.Extensions.DependencyInjection;

namespace GUI.ViewModels;

public partial class MainWindowViewModel : ViewModelBase
{
    private ReplayHandler _replayHandler = new();

    [ObservableProperty] private string? _fileText;
    
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
    private void ParseReplay()
    {
        
    }

    [RelayCommand]
    private void UnhashReplay()
    {
        
    }
    
     

    [RelayCommand]
    private async Task OpenLrfFile(CancellationToken token)
    {
        try
        {
            var fileservice = App.Current?.Services?.GetService<FilesService>();
            if (fileservice is null)
            {
                throw new NullReferenceException("Missing File Service instance.");
            }

            var file = await fileservice.OpenFileAsync(FileType.LRF);
            if (file is null)
            {
                return;
            }

            FileText = file.Path.AbsolutePath;

            
            
            await Task.Run(()=>
            {
                _replayHandler.ParseReplay(FileText, ENetLeagueVersion.Patch420);
                _replayHandler.UnhashReplay();
            }, token);

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