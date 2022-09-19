using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MatchPacketReaderTool.Views;

public partial class FilterView : UserControl
{
    public FilterView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}