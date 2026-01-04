using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Shapes;
using Avalonia.Interactivity;
using Avalonia.Input;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using Avalonia.VisualTree;
using JsonWorkbench.ViewModels;

namespace JsonWorkbench.Views;

public partial class HandbookTabView : UserControl
{
    public HandbookTabView()
    {
        InitializeComponent();
        this.DataContextChanged += (_, _) => WireVm();
        WireVm();

        // Cache selection so toolbar buttons can wrap even if focus/selection collapses.
        var editor = this.FindControl<TextBox>("SourceEditor");
	    if (editor is not null)
	    {
	        // Avalonia TextBox doesn't expose a SelectionChanged event on all versions.
	        // So we cache selection on common user actions.
	        editor.AddHandler(InputElement.PointerReleasedEvent, (_, _) => CacheSelection(editor), RoutingStrategies.Bubble);
	        editor.AddHandler(InputElement.KeyUpEvent, (_, _) => CacheSelection(editor), RoutingStrategies.Bubble);
	        editor.AddHandler(InputElement.PointerPressedEvent, (_, _) => CacheSelection(editor), RoutingStrategies.Bubble);
	    }

        BuildColorWheel();
    }

    private HandbookTabViewModel? _vm;

    // --- Selection cache (robust wrapping) ---
    private int _cachedSelStart;
    private int _cachedSelEnd;
    private bool _cachedHasSelection;

    private void CacheSelection(TextBox editor)
    {
        var start = Math.Min(editor.SelectionStart, editor.SelectionEnd);
        var end = Math.Max(editor.SelectionStart, editor.SelectionEnd);
        if (end > start)
        {
            _cachedSelStart = start;
            _cachedSelEnd = end;
            _cachedHasSelection = true;
            return;
        }

        // If user moved the caret / clicked without selecting, don't keep an old cached selection.
        // This prevents "wrap" actions from unexpectedly targeting stale text.
        _cachedHasSelection = false;
        var caret = Math.Clamp(editor.CaretIndex, 0, (editor.Text ?? string.Empty).Length);
        _cachedSelStart = caret;
        _cachedSelEnd = caret;
    }

    // --- Color wheel state ---
    private const int WheelSize = 204;
    private WriteableBitmap? _wheelBitmap;
    private bool _wheelDragging;
    private double _wheelHue; // 0..1
    private double _wheelSat; // 0..1
    private double _wheelVal = 1.0; // 0..1

    private void WireVm()
    {
        if (_vm is not null)
            _vm.PropertyChanged -= VmOnPropertyChanged;

        _vm = DataContext as HandbookTabViewModel;
        if (_vm is not null)
            _vm.PropertyChanged += VmOnPropertyChanged;

        RenderPreview();
    }

    private void VmOnPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(HandbookTabViewModel.VtmlText) ||
            e.PropertyName == nameof(HandbookTabViewModel.PreviewInlines))
        {
            RenderPreview();
        }

        if (e.PropertyName == nameof(HandbookTabViewModel.PickedColor))
        {
            SyncWheelFromVmColor();
        }
    }

    private void RenderPreview()
    {
        if (_vm is null) return;

        var tb = this.FindControl<TextBlock>("PreviewText");
        if (tb is null) return;

        tb.Inlines.Clear();
        foreach (var i in _vm.PreviewInlines)
            tb.Inlines.Add(i);
    }

    private static TopLevel? GetTop(UserControl c) => c.GetVisualRoot() as TopLevel;

    private async void CopyVtml_Click(object? sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        var clipboard = GetTop(this)?.Clipboard;
        if (clipboard is null) return;

        await clipboard.SetTextAsync(_vm.VtmlText ?? "");
        _vm.Status = "Copied VTML.";
    }

    private async void CopyEscaped_Click(object? sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        var clipboard = GetTop(this)?.Clipboard;
        if (clipboard is null) return;

        await clipboard.SetTextAsync(_vm.JsonEscapedVtml ?? "");
        _vm.Status = "Copied JSON-escaped VTML.";
    }
private void InsertHex_Click(object? sender, RoutedEventArgs e)
{
    if (_vm is null) return;

    // If user has (or recently had) text selected, "Insert Hex" should *apply* that color by wrapping it.
    var editor = this.FindControl<TextBox>("SourceEditor");
    if (editor is null) return;

    var (start, end) = GetBestSelection(editor);
    if (end > start)
    {
        WrapSelection($@"<font color=""{_vm.PickedColorHex}"">", "</font>");
        _vm.Status = "Applied color tag to selection.";
        return;
    }

    // Otherwise just insert the hex at the caret.
    InsertAtCaret(_vm.PickedColorHex);
    _vm.Status = "Inserted hex color.";
}

    private void WrapColor_Click(object? sender, RoutedEventArgs e)
    {
        if (_vm is null) return;
        WrapSelection($@"<font color=""{_vm.PickedColorHex}"">", "</font>");
        _vm.Status = "Wrapped selection with <font color=...>.";
    }

    private void Bold_Click(object? sender, RoutedEventArgs e) => WrapSelection("<strong>", "</strong>");
    private void Italic_Click(object? sender, RoutedEventArgs e) => WrapSelection("<i>", "</i>");

    // Insert-only actions should never replace cached selections.
    private void Br_Click(object? sender, RoutedEventArgs e) => InsertAtCaretInsertOnly("<br>");
    private void Clear_Click(object? sender, RoutedEventArgs e) => InsertAtCaretInsertOnly("<clear>");

    private void Link_Click(object? sender, RoutedEventArgs e)
    {
        // Insert a scaffold and place caret inside href="..."
        var editor = this.FindControl<TextBox>("SourceEditor");
        if (editor is null) return;

        var start = Math.Min(editor.SelectionStart, editor.SelectionEnd);
        var end = Math.Max(editor.SelectionStart, editor.SelectionEnd);
        var selected = (start != end) ? editor.Text?.Substring(start, end - start) ?? "link" : "link";

        var insert = $@"<a href="""">{selected}</a>";
        ReplaceRange(editor, start, end, insert);

        // place caret between the quotes
        var caret = start + "<a href=\"".Length;
        editor.SelectionStart = caret;
        editor.SelectionEnd = caret;
        editor.Focus();
    }

    private void Hotkey_Click(object? sender, RoutedEventArgs e)
    {
        // Example from wiki: <hk>sprint</hk>
        WrapSelection("<hk>", "</hk>");
    }

    private void Icon_Click(object? sender, RoutedEventArgs e)
    {
        InsertAtCaretInsertOnly(@"<icon path=""icons/checkmark.svg""></icon>");
    }

    private void Itemstack_Click(object? sender, RoutedEventArgs e)
    {
        InsertAtCaretInsertOnly(@"<itemstack type=""block"" code=""game:packeddirt"" rsize=""1""></itemstack>");
    }

    private void WrapSelection(string open, string close)
    {
        var editor = this.FindControl<TextBox>("SourceEditor");
        if (editor is null) return;

        var (start, end) = GetBestSelection(editor);

        // If nothing selected, insert a pair and place caret inside.
        if (end <= start)
        {
            var insert = open + close;
            ReplaceRange(editor, start, end, insert);
            var caret = start + open.Length;
            editor.SelectionStart = caret;
            editor.SelectionEnd = caret;
            editor.Focus();
            return;
        }

        var src = editor.Text ?? string.Empty;
        start = Math.Clamp(start, 0, src.Length);
        end = Math.Clamp(end, 0, src.Length);

        var selected = src.Substring(start, end - start);
        var wrapped = open + selected + close;
        ReplaceRange(editor, start, end, wrapped);

        // Keep selection on the original text portion (inside tags)
        var newStart = start + open.Length;
        var newEnd = newStart + selected.Length;
        editor.SelectionStart = newStart;
        editor.SelectionEnd = newEnd;
        editor.Focus();
    }

    private (int start, int end) GetBestSelection(TextBox editor)
    {
        var start = Math.Min(editor.SelectionStart, editor.SelectionEnd);
        var end = Math.Max(editor.SelectionStart, editor.SelectionEnd);
        if (end > start) return (start, end);

        // If selection collapsed (often after clicking a toolbar button), fall back to last cached selection.
        if (_cachedHasSelection)
        {
            var srcLen = (editor.Text ?? string.Empty).Length;
            var cs = Math.Clamp(_cachedSelStart, 0, srcLen);
            var ce = Math.Clamp(_cachedSelEnd, 0, srcLen);
            if (ce > cs) return (cs, ce);
        }

        // Otherwise use caret.
        var caret = Math.Clamp(editor.CaretIndex, 0, (editor.Text ?? string.Empty).Length);
        return (caret, caret);
    }

    private void InsertAtCaret(string text)
    {
        var editor = this.FindControl<TextBox>("SourceEditor");
        if (editor is null) return;

        var (start, end) = GetBestSelection(editor);
        ReplaceRange(editor, start, end, text);

        var caret = start + text.Length;
        editor.SelectionStart = caret;
        editor.SelectionEnd = caret;
        editor.Focus();
    }

    /// <summary>
    /// Inserts text at the caret position without replacing any selection or cached selection.
    /// Useful for scaffolds like <br>, <clear>, <icon>, <itemstack>.
    /// </summary>
    private void InsertAtCaretInsertOnly(string text)
    {
        var editor = this.FindControl<TextBox>("SourceEditor");
        if (editor is null) return;

        var src = editor.Text ?? string.Empty;
        var caret = Math.Clamp(editor.CaretIndex, 0, src.Length);

        editor.Text = src.Substring(0, caret) + text + src.Substring(caret);
        var newCaret = caret + text.Length;
        editor.SelectionStart = newCaret;
        editor.SelectionEnd = newCaret;
        editor.CaretIndex = newCaret;
        editor.Focus();
    }

    private static void ReplaceRange(TextBox editor, int start, int end, string replacement)
    {
        var src = editor.Text ?? string.Empty;
        start = Math.Clamp(start, 0, src.Length);
        end = Math.Clamp(end, 0, src.Length);
        if (end < start) (start, end) = (end, start);

        editor.Text = src.Substring(0, start) + replacement + src.Substring(end);
    }

    // ----------------------------
    // Color wheel implementation
    // ----------------------------

    private void BuildColorWheel()
    {
        var img = this.FindControl<Image>("ColorWheelImage");
        if (img is null) return;

        _wheelBitmap = new WriteableBitmap(
            new PixelSize(WheelSize, WheelSize),
            new Vector(96, 96),
            PixelFormat.Bgra8888,
            AlphaFormat.Premul);

	    using (var fb = _wheelBitmap.Lock())
	    {
	        // Avoid /unsafe compilation: render into a managed buffer and Marshal.Copy into the bitmap.
	        var stride = fb.RowBytes;
	        var buffer = new byte[stride * WheelSize];
	        var cx = (WheelSize - 1) / 2.0;
	        var cy = (WheelSize - 1) / 2.0;
	        var radius = Math.Min(cx, cy);

	        for (var y = 0; y < WheelSize; y++)
	        {
	            for (var x = 0; x < WheelSize; x++)
	            {
	                var dx = x - cx;
	                var dy = y - cy;
	                var r = Math.Sqrt(dx * dx + dy * dy);
	                var offset = y * stride + x * 4;

	                if (r > radius)
	                {
	                    buffer[offset + 0] = 0;
	                    buffer[offset + 1] = 0;
	                    buffer[offset + 2] = 0;
	                    buffer[offset + 3] = 0;
	                    continue;
	                }

	                var sat = r / radius;
	                var hue = (Math.Atan2(dy, dx) / (2 * Math.PI) + 1.0) % 1.0;
	                var c = HsvToRgb(hue, sat, 1.0);
	
	                // BGRA
	                buffer[offset + 0] = c.B;
	                buffer[offset + 1] = c.G;
	                buffer[offset + 2] = c.R;
	                buffer[offset + 3] = 255;
	            }
	        }

	        Marshal.Copy(buffer, 0, fb.Address, buffer.Length);
	    }

        img.Source = _wheelBitmap;
        SyncWheelFromVmColor();
    }

    private void SyncWheelFromVmColor()
    {
        if (_vm is null) return;
        var (h, s, v) = RgbToHsv(_vm.PickedColor);
        _wheelHue = h;
        _wheelSat = s;
        // Keep current value slider if user changed it; otherwise adopt from color.
        if (!_wheelDragging)
            _wheelVal = v;

        var slider = this.FindControl<Slider>("ValueSlider");
        if (slider is not null && !_wheelDragging)
            slider.Value = Math.Round(_wheelVal * 100);

        UpdateWheelMarker();
    }

    private void UpdateWheelMarker()
    {
        var marker = this.FindControl<Ellipse>("ColorWheelMarker");
        if (marker is null) return;

        var cx = (WheelSize - 1) / 2.0;
        var cy = (WheelSize - 1) / 2.0;
        var radius = Math.Min(cx, cy);
        var angle = _wheelHue * 2 * Math.PI;
        var r = _wheelSat * radius;

        var x = cx + Math.Cos(angle) * r;
        var y = cy + Math.Sin(angle) * r;

        Canvas.SetLeft(marker, x - marker.Width / 2);
        Canvas.SetTop(marker, y - marker.Height / 2);
    }

    private void ApplyWheelColorToVm()
    {
        if (_vm is null) return;
        var c = HsvToRgb(_wheelHue, _wheelSat, _wheelVal);
        _vm.PickedColor = c;
    }

    private void UpdateWheelFromPointer(PointerEventArgs e)
    {
        var img = this.FindControl<Image>("ColorWheelImage");
        if (img is null) return;

        var p = e.GetPosition(img);
        var cx = (WheelSize - 1) / 2.0;
        var cy = (WheelSize - 1) / 2.0;
        var dx = p.X - cx;
        var dy = p.Y - cy;
        var radius = Math.Min(cx, cy);
        var r = Math.Sqrt(dx * dx + dy * dy);
        if (r > radius) return;

        _wheelSat = Math.Clamp(r / radius, 0, 1);
        _wheelHue = (Math.Atan2(dy, dx) / (2 * Math.PI) + 1.0) % 1.0;
        UpdateWheelMarker();
        ApplyWheelColorToVm();
    }

    private void ColorWheel_PointerPressed(object? sender, PointerPressedEventArgs e)
    {
        _wheelDragging = true;
        UpdateWheelFromPointer(e);
        e.Pointer.Capture(sender as IInputElement);
    }

    private void ColorWheel_PointerMoved(object? sender, PointerEventArgs e)
    {
        if (!_wheelDragging) return;
	    var vis = sender as Visual;
	    if (vis is null) return;
	    var pt = e.GetCurrentPoint(vis);
	    if (pt.Properties.IsLeftButtonPressed)
            UpdateWheelFromPointer(e);
    }

	private void CopyItemHandbookJson_Click(object? sender, RoutedEventArgs e)
	{
	    if (_vm is null) return;
	    var clipboard = GetTop(this)?.Clipboard;
	    if (clipboard is null) return;

	    clipboard.SetTextAsync(_vm.ItemHandbookJson ?? "");
	    _vm.Status = "Copied item handbook JSON.";
	}

	private void CopyGuidePageJson_Click(object? sender, RoutedEventArgs e)
	{
	    if (_vm is null) return;
	    var clipboard = GetTop(this)?.Clipboard;
	    if (clipboard is null) return;

	    clipboard.SetTextAsync(_vm.GuidePageJson ?? "");
	    _vm.Status = "Copied guide page JSON.";
	}

    private void ValueSlider_ValueChanged(object? sender, RangeBaseValueChangedEventArgs e)
    {
        _wheelVal = Math.Clamp(e.NewValue / 100.0, 0, 1);
        ApplyWheelColorToVm();
    }

    // HSV helpers
    private static Color HsvToRgb(double h, double s, double v)
    {
        h = (h % 1.0 + 1.0) % 1.0;
        s = Math.Clamp(s, 0, 1);
        v = Math.Clamp(v, 0, 1);

        var c = v * s;
        var x = c * (1 - Math.Abs((h * 6) % 2 - 1));
        var m = v - c;

        double r1, g1, b1;
        var hp = h * 6;
        if (hp < 1) { r1 = c; g1 = x; b1 = 0; }
        else if (hp < 2) { r1 = x; g1 = c; b1 = 0; }
        else if (hp < 3) { r1 = 0; g1 = c; b1 = x; }
        else if (hp < 4) { r1 = 0; g1 = x; b1 = c; }
        else if (hp < 5) { r1 = x; g1 = 0; b1 = c; }
        else { r1 = c; g1 = 0; b1 = x; }

        byte R = (byte)Math.Round((r1 + m) * 255);
        byte G = (byte)Math.Round((g1 + m) * 255);
        byte B = (byte)Math.Round((b1 + m) * 255);
        return Color.FromRgb(R, G, B);
    }

    private static (double h, double s, double v) RgbToHsv(Color c)
    {
        var r = c.R / 255.0;
        var g = c.G / 255.0;
        var b = c.B / 255.0;

        var max = Math.Max(r, Math.Max(g, b));
        var min = Math.Min(r, Math.Min(g, b));
        var delta = max - min;

        double h;
        if (delta == 0) h = 0;
        else if (max == r) h = ((g - b) / delta) % 6;
        else if (max == g) h = ((b - r) / delta) + 2;
        else h = ((r - g) / delta) + 4;

        h /= 6;
        if (h < 0) h += 1;

        var s = max == 0 ? 0 : delta / max;
        var v = max;
        return (h, s, v);
    }

    protected override void OnPointerReleased(PointerReleasedEventArgs e)
    {
        base.OnPointerReleased(e);
        _wheelDragging = false;
    }

    private void FontInsert_Click(object? sender, RoutedEventArgs e)
    {
        if (_vm is null) return;

        var editor = this.FindControl<TextBox>("SourceEditor");
        if (editor is null) return;

        var (open, close) = BuildFontTags();

        // Insert template at caret, do NOT wrap selection.
        InsertTemplateAtCaret(editor, open, close);
        _vm.Status = "Inserted <font> template.";
    }

    private void FontWrap_Click(object? sender, RoutedEventArgs e)
    {
        if (_vm is null) return;

        var (open, close) = BuildFontTags();
        WrapSelection(open, close);
        _vm.Status = "Applied <font> tags.";
    }

    private (string open, string close) BuildFontTags()
    {
        var sizeText = this.FindControl<TextBox>("FontSizeBox")?.Text?.Trim();
        var weight = GetComboValue("FontWeightBox");
        var align = GetComboValue("FontAlignBox");
        var opacityText = this.FindControl<TextBox>("FontOpacityBox")?.Text?.Trim();
        var lineHeightText = this.FindControl<TextBox>("FontLineHeightBox")?.Text?.Trim();
        var useColor = this.FindControl<CheckBox>("FontUseColorBox")?.IsChecked ?? true;

        var attrs = new System.Text.StringBuilder();

        if (useColor && _vm is not null)
            attrs.Append($" color=\"{_vm.PickedColorHex}\"");

        if (int.TryParse(sizeText, out var size) && size > 0)
            attrs.Append($" size=\"{size}\"");

        if (!string.IsNullOrWhiteSpace(weight) && weight != "normal")
            attrs.Append($" weight=\"{weight}\"");

        if (!string.IsNullOrWhiteSpace(align) && align != "left")
            attrs.Append($" align=\"{align}\"");

        if (double.TryParse(opacityText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var op))
        {
            op = Math.Clamp(op, 0.0, 1.0);
            if (op < 1.0)
                attrs.Append($" opacity=\"{op.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}\"");
        }

        if (double.TryParse(lineHeightText, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var lh) && lh > 0)
            attrs.Append($" lineheight=\"{lh.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture)}\"");

        var open = "<font" + attrs + ">";
        return (open, "</font>");
    }

    private string GetComboValue(string name)
    {
        var cb = this.FindControl<ComboBox>(name);
        if (cb?.SelectedItem is ComboBoxItem cbi && cbi.Content is string s)
            return s.Trim();

        return (cb?.SelectedItem?.ToString() ?? "").Trim();
    }

    private static void InsertTemplateAtCaret(TextBox editor, string open, string close)
    {
        var src = editor.Text ?? string.Empty;
        var caret = Math.Clamp(editor.CaretIndex, 0, src.Length);

        var insert = open + close;
        editor.Text = src.Substring(0, caret) + insert + src.Substring(caret);

        var innerCaret = caret + open.Length;
        editor.SelectionStart = innerCaret;
        editor.SelectionEnd = innerCaret;
        editor.CaretIndex = innerCaret;
        editor.Focus();
    }

    private void TagItem_PointerEnter(object? sender, PointerEventArgs e)
    {
        if (_vm is null) return;
        if (sender is Control c && c.DataContext is TagInfo info)
            _vm.HoveredTag = info;
    }

}
