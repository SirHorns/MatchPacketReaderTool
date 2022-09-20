using System;
using Avalonia.Controls;
using MatchPacketReaderTool.ViewModels;

namespace MatchPacketReaderTool.Services;

public class ViewLocator
{
    /// <summary>
    /// Finds a view from a given ViewModel
    /// </summary>
    /// <param name="vm">The ViewModel representing a View</param>
    /// <returns>The View that matches the ViewModel. Null is no match found</returns>
    public static Window ResolveViewFromViewModel<T>(T vm) where T : ViewModelBase
    {
        var name = vm.GetType().AssemblyQualifiedName!.Replace("ViewModel", "View");
        var type = Type.GetType(name);
        return type != null ? (Window)Activator.CreateInstance(type)! : null;
    }
}