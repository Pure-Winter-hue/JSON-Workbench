namespace JsonWorkbench.ViewModels;

/// <summary>
/// Simple marker interface so the TabControl header can bind to a common Title.
/// </summary>
public interface ITabViewModel
{
    string Title { get; }
}
