using System.Collections.ObjectModel;

namespace JsonWorkbench.ViewModels;

public sealed class MainWindowViewModel
{
    public ObservableCollection<ITabViewModel> Tabs { get; } =
    [
        WorkbenchTabViewModel.CreateBlockTab(),
        WorkbenchTabViewModel.CreateItemTab(),
        WorkbenchTabViewModel.CreateEntityTab(),
        WorkbenchTabViewModel.CreateRecipeTab(),
        new HandbookTabViewModel()
    ];

    public ITabViewModel SelectedTab { get; set; }

    public MainWindowViewModel()
    {
        SelectedTab = Tabs[0];
    }
}
