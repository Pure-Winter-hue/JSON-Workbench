using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Markup.Xaml.Styling;
using Avalonia.Styling;
using System;
using System.Linq;
using JsonWorkbench.ViewModels;
using JsonWorkbench.Views;

namespace JsonWorkbench;

public partial class App : Application
{
    private StyleInclude? _themeStyles;

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        // Grab the theme include that App.axaml seeded so we can swap it cleanly.
        _themeStyles = Styles.OfType<StyleInclude>()
            .FirstOrDefault(s => s.Source is not null && s.Source.OriginalString.Contains("/Themes/"));

        // Ensure we start in Light regardless of OS theme. >_> PW Has preferences.
        SetTheme("Light");
    }

    public void SetTheme(string themeName)
    {
        // Keep FluentTheme in charge of control templates; we just swap our small override palette.
        var (variant, source) = themeName switch
        {
            "Nord" => (ThemeVariant.Dark, new Uri("avares://JsonWorkbench/Themes/Nord.axaml")),
            "Dracula" => (ThemeVariant.Dark, new Uri("avares://JsonWorkbench/Themes/Dracula.axaml")),
            "Monokai" => (ThemeVariant.Dark, new Uri("avares://JsonWorkbench/Themes/Monokai.axaml")),
            _ => (ThemeVariant.Light, new Uri("avares://JsonWorkbench/Themes/Light.axaml")),
        };

        RequestedThemeVariant = variant;

        // Remove the previous theme include and add the new one.
        if (_themeStyles != null)
            Styles.Remove(_themeStyles);

        _themeStyles = new StyleInclude(new Uri("avares://JsonWorkbench/"))
        {
            Source = source
        };

        Styles.Add(_themeStyles);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel()
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
