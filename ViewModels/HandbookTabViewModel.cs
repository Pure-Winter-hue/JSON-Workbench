using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using Avalonia.Media;

namespace JsonWorkbench.ViewModels;

public sealed class HandbookTabViewModel : INotifyPropertyChanged, ITabViewModel
{
    public event PropertyChangedEventHandler? PropertyChanged;

    public string Title { get; } = "Handbook";

    private string _status = "";
    public string Status
    {
        get => _status;
        set { _status = value ?? ""; OnPropertyChanged(nameof(Status)); }
    }

    private string _vtmlText =
        "Welcome to <strong>VTML</strong>!<br>" +
        "Use <i>italics</i>, <font color=\"#ffcc00\" weight=\"bold\">color</font>, " +
        "and <a href=\"handbook://survival\">links</a>.";

    public string VtmlText
    {
        get => _vtmlText;
        set
        {
            _vtmlText = value ?? "";
            OnPropertyChanged(nameof(VtmlText));
            OnPropertyChanged(nameof(PreviewInlines));
            OnPropertyChanged(nameof(JsonEscapedVtml));
        }
    }

    /// <summary>
    /// Live preview content (inlines) regenerated whenever VtmlText changes.
    /// </summary>
    public IReadOnlyList<Avalonia.Controls.Documents.Inline> PreviewInlines
        => VtmlRenderer.Render(VtmlText);

    private Color _pickedColor = Color.FromRgb(0xFF, 0xCC, 0x00);
    public Color PickedColor
    {
        get => _pickedColor;
        set
        {
            _pickedColor = value;
            OnPropertyChanged(nameof(PickedColor));
            OnPropertyChanged(nameof(PickedColorHex));
            OnPropertyChanged(nameof(PickedR));
            OnPropertyChanged(nameof(PickedG));
            OnPropertyChanged(nameof(PickedB));
            OnPropertyChanged(nameof(PickedBrush));
        }
    }

    public IBrush PickedBrush => new SolidColorBrush(PickedColor);

    public int PickedR
    {
        get => PickedColor.R;
        set => PickedColor = Color.FromRgb((byte)ClampByte(value), PickedColor.G, PickedColor.B);
    }

    public int PickedG
    {
        get => PickedColor.G;
        set => PickedColor = Color.FromRgb(PickedColor.R, (byte)ClampByte(value), PickedColor.B);
    }

    public int PickedB
    {
        get => PickedColor.B;
        set => PickedColor = Color.FromRgb(PickedColor.R, PickedColor.G, (byte)ClampByte(value));
    }

    public string PickedColorHex => VtmlRenderer.ToHexRgb(PickedColor);

    /// <summary>
    /// VTML escaped as a JSON string value (no surrounding key).
    /// </summary>
    public string JsonEscapedVtml => EscapeAsJsonString(VtmlText);


    // ----------------------------
    // JSON helpers
    // ----------------------------
    private bool _itemHandbookInclude = true;
    public bool ItemHandbookInclude
    {
        get => _itemHandbookInclude;
        set
        {
            _itemHandbookInclude = value;
            OnPropertyChanged(nameof(ItemHandbookInclude));
            OnPropertyChanged(nameof(ItemHandbookJson));
        }
    }

    private string _itemHandbookTitleKey = "myitem-handbook-title";
    public string ItemHandbookTitleKey
    {
        get => _itemHandbookTitleKey;
        set
        {
            _itemHandbookTitleKey = value ?? "";
            OnPropertyChanged(nameof(ItemHandbookTitleKey));
            OnPropertyChanged(nameof(ItemHandbookJson));
        }
    }

    private string _itemHandbookTextKey = "myitem-handbook-text";
    public string ItemHandbookTextKey
    {
        get => _itemHandbookTextKey;
        set
        {
            _itemHandbookTextKey = value ?? "";
            OnPropertyChanged(nameof(ItemHandbookTextKey));
            OnPropertyChanged(nameof(ItemHandbookJson));
        }
    }

    /// <summary>
    /// JSON snippet pastable into an itemtype/blocktype. Uses handbook.include + handbook.extraSections.
    /// (title/text are lang keys).
    /// </summary>
    public string ItemHandbookJson
    {
        get
        {
            var include = ItemHandbookInclude ? "true" : "false";
            var title = EscapeJson(ItemHandbookTitleKey);
            var text = EscapeJson(ItemHandbookTextKey);
            return
$"\"handbook\": {{\n" +
$"  \"include\": {include},\n" +
$"  \"extraSections\": [\n" +
$"    {{ \"title\": \"{title}\", \"text\": \"{text}\" }}\n" +
$"  ]\n" +
$"}}";
        }
    }

    private string _guidePageCode = "mymod:introduction";
    public string GuidePageCode
    {
        get => _guidePageCode;
        set
        {
            _guidePageCode = value ?? "";
            OnPropertyChanged(nameof(GuidePageCode));
            OnPropertyChanged(nameof(GuidePageJson));
        }
    }

    private string _guideTitleKey = "mymod:introduction-title";
    public string GuideTitleKey
    {
        get => _guideTitleKey;
        set
        {
            _guideTitleKey = value ?? "";
            OnPropertyChanged(nameof(GuideTitleKey));
            OnPropertyChanged(nameof(GuidePageJson));
        }
    }

    private string _guideTextKey = "mymod:introduction-text";
    public string GuideTextKey
    {
        get => _guideTextKey;
        set
        {
            _guideTextKey = value ?? "";
            OnPropertyChanged(nameof(GuideTextKey));
            OnPropertyChanged(nameof(GuidePageJson));
        }
    }

    private string _guideCategoryCode = "";
    public string GuideCategoryCode
    {
        get => _guideCategoryCode;
        set
        {
            _guideCategoryCode = value ?? "";
            OnPropertyChanged(nameof(GuideCategoryCode));
            OnPropertyChanged(nameof(GuidePageJson));
        }
    }

    /// <summary>
    /// JSON file content for assets/&lt;modid&gt;/config/handbook/&lt;something&gt;.json
    /// </summary>
    public string GuidePageJson
    {
        get
        {
            var pageCode = EscapeJson(GuidePageCode);
            var title = EscapeJson(GuideTitleKey);
            var text = EscapeJson(GuideTextKey);
            var cat = (GuideCategoryCode ?? "").Trim();
            var categoryLine = string.IsNullOrWhiteSpace(cat) ? "" : $",\n  \"categoryCode\": \"{EscapeJson(cat)}\"";

            return
$"{{\n" +
$"  \"pageCode\": \"{pageCode}\",\n" +
$"  \"title\": \"{title}\",\n" +
$"  \"text\": \"{text}\"" +
$"{categoryLine}\n" +
$"}}";
        }
    }

    
    private TagInfo? _hoveredTag;
    public TagInfo? HoveredTag
    {
        get => _hoveredTag;
        set
        {
            _hoveredTag = value;
            OnPropertyChanged(nameof(HoveredTag));
            OnPropertyChanged(nameof(HoveredTagTag));
            OnPropertyChanged(nameof(HoveredTagWhatItDoes));
            OnPropertyChanged(nameof(HoveredTagExample));
        }
    }

    public string HoveredTagTag => HoveredTag?.Tag ?? "Hover a tag below";
    public string HoveredTagWhatItDoes => HoveredTag?.WhatItDoes ?? "Move your mouse over a tag to see what it does and an example.";
    public string HoveredTagExample => HoveredTag?.Example ?? "";

public ObservableCollection<TagInfo> Tags { get; } =
    [
        new("<br>", "Line break.", "First line<br>Second line"),
        new("<i>...</i>", "Italic text.", "<i>italic</i>"),
        new("<strong>...</strong>", "Bold text.", "<strong>bold</strong>"),
        new("<a href=\"...\">...</a>", "Clickable link (web, handbook://..., command://..., hotkey://..., etc.).", "<a href=\"http://example.com\">link</a>"),
        new("<font ...>...</font>", "Text styling: size, color, weight, opacity, align, lineheight.", "<font color=\"#ffcc00\" size=\"18\" weight=\"bold\">styled</font>"),
        new("<hk>...</hk>", "Hotkey display. In-game it resolves to the player's bound key.", "Press <hk>sprint</hk>"),
        new("<itemstack ...></itemstack>", "Renders an item/block stack (preview uses a placeholder).", "<itemstack type=\"block\" code=\"game:packeddirt\" rsize=\"1\"></itemstack>"),
        new("<icon ...></icon>", "Renders an icon (preview uses a placeholder).", "<icon path=\"icons/checkmark.svg\"></icon>"),
        new("<clear>", "Moves next line below floated elements.", "<clear>")
    ];

    private void OnPropertyChanged(string name) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

    private static int ClampByte(int v) => v < 0 ? 0 : (v > 255 ? 255 : v);

    private static string EscapeJson(string s) => EscapeAsJsonString(s ?? "");

    private static string EscapeAsJsonString(string s)
    {
        // Produces a JSON string literal without surrounding quotes, suitable for showing in UI.
        // Escapes backslashes, quotes, and control chars.
        var sb = new System.Text.StringBuilder(s.Length + 16);
        foreach (var ch in s)
        {
            switch (ch)
            {
                case '\\': sb.Append("\\\\"); break;
                case '"': sb.Append("\\\""); break;
                case '\n': sb.Append("\\n"); break;
                case '\r': sb.Append("\\r"); break;
                case '\t': sb.Append("\\t"); break;
                default:
                    if (ch < 0x20)
                        sb.Append("\\u").Append(((int)ch).ToString("x4"));
                    else
                        sb.Append(ch);
                    break;
            }
        }
        return sb.ToString();
    }

}
