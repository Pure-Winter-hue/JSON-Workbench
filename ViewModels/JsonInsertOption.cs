namespace JsonWorkbench.ViewModels;

public enum InsertMode
{
    SetIfMissing,
    Overwrite,
    MergeObject,
    AppendToArray,
    ReplaceRoot
}

public sealed record JsonInsertOption(
    string Category,
    string DisplayName,
    string Pointer,
    InsertMode Mode,
    string TemplateJson,
    string Description
);
