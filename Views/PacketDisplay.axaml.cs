using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MatchPacketReaderTool.Views;

public partial class PacketDisplay : UserControl
{
    public PacketDisplay()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}