using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MatchPacketReaderTool.Views;

public partial class ItemV : UserControl
{
    public ItemV()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}