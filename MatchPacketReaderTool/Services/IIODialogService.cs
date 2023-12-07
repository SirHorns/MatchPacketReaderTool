using System.Threading.Tasks;
using Avalonia.Controls;
using MatchPacketReaderTool.ViewModels;

namespace MatchPacketReaderTool.Services;

/// <summary>
/// An interface that represents working with IO dialogs
/// </summary>
public interface IIODialogService
{
    /// <summary>
    /// Shows an open file dialog
    /// </summary>
    /// <param name="title">The title for the dialog</param>
    /// <param name="filter">The filter of file types allowed by the dialog</param>
    /// <returns>The path of the file that was opened. Null if no file was opened</returns>
    Task<string> ShowOpenFileDialogAsync(ViewModelBase parent, string title, FileDialogFilter filter);

    /// <summary>
    /// Shows an open folder dialog
    /// </summary>
    /// <param name="title">The title for the dialog</param>
    /// <returns>The path of the folder that was opened. Null if no folder was opened</returns>
    Task<string> ShowOpenFolderDialogAsync(ViewModelBase parent, string title);

    /// <summary>
    /// Shows a save file dialog
    /// </summary>
    /// <param name="title">The title for the dialog</param>
    /// <param name="filter">The filter of file types allowed by the dialog</param>
    /// <returns>The path of the file that was saved. Null if no file was saved</returns>
    Task<string> ShowSaveFileDialogAsync(ViewModelBase parent, string title, FileDialogFilter filter);
}