using System;

namespace MatchPacketReaderTool.ViewModels;

public class PacketTypeViewModel : ViewModelBase
{
    public PacketTypeViewModel(string type)
    {
        Type = type;
    }

    public string Type { get; }
}