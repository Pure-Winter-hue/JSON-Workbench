using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace JsonWorkbench.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
    }

    private void SetTheme(string themeName)
    {
        if (Application.Current is JsonWorkbench.App app)
            app.SetTheme(themeName);
    }

    // MenuItem.Click uses RoutedEventArgs.
    public void ThemeLight_Click(object? sender, RoutedEventArgs e) => SetTheme("Light");
    public void ThemeNord_Click(object? sender, RoutedEventArgs e) => SetTheme("Nord");
    public void ThemeDracula_Click(object? sender, RoutedEventArgs e) => SetTheme("Dracula");
    public void ThemeMonokai_Click(object? sender, RoutedEventArgs e) => SetTheme("Monokai");
}
