using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.ReactiveUI;
using MatchPacketReaderTool.ViewModels;
using ReactiveUI;

namespace MatchPacketReaderTool.Views
{
    public partial class MainWindow : ReactiveWindow<MainWindowViewModel>
    {
        public MainWindow()
        {
            InitializeComponent();
            this.WhenActivated(d => d(ViewModel!.ShowDiag.RegisterHandler(DoShowDialogAsync)));
        }
        
        private async Task DoShowDialogAsync(InteractionContext<string, string> interaction)
        {
            OpenFileDialog _dialog = new OpenFileDialog();
            _dialog.Filters.Add(new FileDialogFilter() { Name = "JSON", Extensions =  { "json" } });
            _dialog.AllowMultiple = false;

             var _result = await _dialog.ShowAsync(this);

             if (_result != null)
             {
                 interaction.SetOutput(string.Join(" ", _result));
             }
        }

        private async Task<string> GetPath()
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Filters.Add(new FileDialogFilter() { Name = "Text", Extensions =  { "txt" } });

            string[] result = await dialog.ShowAsync(this);

            return string.Join(" ", result);
        }
    }
}