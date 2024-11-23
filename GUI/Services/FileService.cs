using System;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using GUI.Enums;

namespace GUI.Services;

public class FilesService
{
    private readonly Window _target;

    public FilesService(Window target)
    {
        _target = target;
    }

    internal async Task<IStorageFile?> OpenReplayFileAsync(FileType type)
    {
        var fpo = new FilePickerOpenOptions()
        {
            Title = "Open .lrf File",
            AllowMultiple = false
        };

        switch (type)
        {
            case FileType.LRF:
                fpo.FileTypeFilter = [new FilePickerFileType(".lrf") { Patterns = ["*.lrf"] }];
                break;
            case FileType.RLRF:
                fpo.FileTypeFilter = [new FilePickerFileType(".plrf") { Patterns = ["*.plrf"] }];
                break;
            case FileType.JSON:
                fpo.FileTypeFilter = [new FilePickerFileType(".json") { Patterns = ["*.json"] }];
                break;
            default:
                return null;
        }
        
        var files = await _target.StorageProvider.OpenFilePickerAsync(fpo);

        return files.Count >= 1 ? files[0] : null;
    }
    
    internal async Task<IStorageFile?> OpenPlrfAsync()
    {
        var files = await _target.StorageProvider.OpenFilePickerAsync(new FilePickerOpenOptions()
        {
            Title = "Open .plrf File",
            AllowMultiple = false,
            FileTypeFilter = new FilePickerFileType[]
            {
                new(".plrf")
                {
                    Patterns = new []{"*.plrf"}
                }
            }
        });

        return files.Count >= 1 ? files[0] : null;
    }

    internal async Task<IStorageFile?> SaveFileAsync()
    {
        return await _target.StorageProvider.SaveFilePickerAsync(new FilePickerSaveOptions()
        {
            Title = "Save Text File"
        });
    }
}