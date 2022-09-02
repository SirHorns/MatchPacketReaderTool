using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace MatchPacketReaderTool.Views;

public partial class PacketTypeView : UserControl
{
    public PacketTypeView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}