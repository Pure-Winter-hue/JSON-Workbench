using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;

namespace JsonWorkbench.ViewModels;

public sealed class WorkbenchTabViewModel : INotifyPropertyChanged, ITabViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title { get; }
    public ObservableCollection<string> Categories { get; }
    public ObservableCollection<JsonInsertOption> AllOptions { get; }

    private string _jsonText = "{\n}\n";
    public string JsonText
    {
        get => _jsonText;
        set
        {
            _jsonText = value ?? "";
            Status = "";
            OnPropertyChanged(nameof(JsonText));
        }
    }

    private string _status = "";
    public string Status
    {
        get => _status;
        set { _status = value ?? ""; OnPropertyChanged(nameof(Status)); }
    }

    private string _selectedCategory;
    public string SelectedCategory
    {
        get => _selectedCategory;
        set
        {
            _selectedCategory = value;
            OnPropertyChanged(nameof(SelectedCategory));
            OnPropertyChanged(nameof(FilteredOptions));
        }
    }

    private JsonInsertOption? _selectedOption;
    public JsonInsertOption? SelectedOption
    {
        get => _selectedOption;
        set { _selectedOption = value; OnPropertyChanged(nameof(SelectedOption)); }
    }

    public IEnumerable<JsonInsertOption> FilteredOptions =>
        SelectedCategory == "All"
            ? AllOptions
            : AllOptions.Where(o => o.Category == SelectedCategory);

    private WorkbenchTabViewModel(string title, IEnumerable<JsonInsertOption> options, string initialJson)
    {
        Title = title;
        AllOptions = new ObservableCollection<JsonInsertOption>(options);

        var cats = options.Select(o => o.Category).Distinct().OrderBy(x => x).ToList();
        Categories = new ObservableCollection<string>(new[] { "All" }.Concat(cats));

        _selectedCategory = "All";
        _selectedOption = AllOptions.FirstOrDefault();

        _jsonText = initialJson;
    }

    public void ApplySelectedOption()
    {
        if (SelectedOption is null)
        {
            Status = "Pick an option first.";
            return;
        }

        var (newJson, error) = JsonWorkbenchEngine.Apply(JsonText, SelectedOption);
        if (error is not null)
        {
            Status = error;
            return;
        }

        JsonText = newJson;
        Status = $"Added: {SelectedOption.DisplayName}";
    }

    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    // Factory tabs
    public static WorkbenchTabViewModel CreateItemTab() =>
        new("Item", OptionCatalog.ItemOptions(), "{\n  \"code\": \"myitem\"\n}\n");

    public static WorkbenchTabViewModel CreateBlockTab() =>
        new("Block", OptionCatalog.BlockOptions(), "{\n  \"code\": \"myblock\"\n}\n");

    public static WorkbenchTabViewModel CreateEntityTab() =>
        new("Entity", OptionCatalog.EntityOptions(), "{\n  \"code\": \"myentity\"\n}\n");

    public static WorkbenchTabViewModel CreateRecipeTab() =>
        new("Recipe", OptionCatalog.RecipeOptions(), "{\n}\n");
}
