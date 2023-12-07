using System.Threading.Tasks;
using Avalonia.Controls;
using MatchPacketReaderTool.ViewModels;

namespace MatchPacketReaderTool.Services;

/// <summary>
    /// A service that contains methods for working with IO dialogs
    /// </summary>
    public class IODialogService : IIODialogService
    {
        /// <summary>
        /// Shows an open file dialog
        /// </summary>
        /// <param name="title">The title for the dialog</param>
        /// <param name="filter">The filter of file types allowed by the dialog</param>
        /// <returns>The path of the file that was opened. Null if no file was opened</returns>
        public async Task<string> ShowOpenFileDialogAsync(ViewModelBase parent, string title, FileDialogFilter filter)
        {
            var openFileDialog = new OpenFileDialog()
            {
                Title = title,
                AllowMultiple = false
            };
            openFileDialog.Filters.Add(filter);
            var result = await openFileDialog.ShowAsync(ViewLocator.ResolveViewFromViewModel(parent));
            return result[0];
        }

        /// <summary>
        /// Shows an open folder dialog
        /// </summary>
        /// <param name="title">The title for the dialog</param>
        /// <returns>The path of the folder that was opened. Null if no folder was opened</returns>
        public async Task<string> ShowOpenFolderDialogAsync(ViewModelBase parent, string title)
        {
            var openFolderDialog = new OpenFolderDialog()
            {
                Title = title
            };
            return await openFolderDialog.ShowAsync(ViewLocator.ResolveViewFromViewModel(parent));
        }

        /// <summary>
        /// Shows a save file dialog
        /// </summary>
        /// <param name="title">The title for the dialog</param>
        /// <param name="filter">The filter of file types allowed by the dialog</param>
        /// <returns>The path of the file that was saved. Null if no file was saved</returns>
        public async Task<string> ShowSaveFileDialogAsync(ViewModelBase parent, string title, FileDialogFilter filter)
        {
            var saveFileDialog = new SaveFileDialog()
            {
                Title = title
            };
            saveFileDialog.Filters.Add(filter);
            return await saveFileDialog.ShowAsync(ViewLocator.ResolveViewFromViewModel(parent));
        }
    }