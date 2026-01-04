using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using JsonWorkbench.ViewModels;

namespace JsonWorkbench.Views;

public partial class WorkbenchTabView : UserControl
{
    public WorkbenchTabView()
    {
        InitializeComponent();
    }

    private async void CopyJson_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not WorkbenchTabViewModel vm) return;

        var top = this.GetVisualRoot() as TopLevel;
        var clipboard = top?.Clipboard;
        if (clipboard is null) return;

        await clipboard.SetTextAsync(vm.JsonText ?? "");
        vm.Status = "Copied to clipboard.";
    }

    private void Add_Click(object? sender, RoutedEventArgs e)
    {
        if (DataContext is not WorkbenchTabViewModel vm) return;
        vm.ApplySelectedOption();
    }
}
