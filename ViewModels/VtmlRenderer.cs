using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace JsonWorkbench.ViewModels;

/// <summary>
/// Tiny VTML -> Avalonia inlines renderer.
/// Supports the most common tags used in handbook / descriptions.
/// Anything unknown is rendered as plain text.
///
/// Supported (PW Grabbed from the VS wiki):
/// br, i, strong, a[href], font[size,color,weight,opacity,lineheight,align], hk/hotkey, clear,
/// icon (placeholder), itemstack (placeholder)
/// </summary>
public static class VtmlRenderer
{
    private sealed record StyleState(
        bool Italic,
        bool Bold,
        Color? Color,
        double? FontSize,
        double? Opacity,
        string? Link,
        bool Monospace,
        string? HotkeyCode
    );

    private sealed record TagFrame(string Name, StyleState Previous);

    public static IReadOnlyList<Inline> Render(string? vtml)
    {
        var text = vtml ?? string.Empty;

        var inlines = new List<Inline>(256);
        var state = new StyleState(
            Italic: false,
            Bold: false,
            Color: null,
            FontSize: null,
            Opacity: null,
            Link: null,
            Monospace: false,
            HotkeyCode: null
        );

        var stack = new Stack<TagFrame>();

        int i = 0;
        while (i < text.Length)
        {
            var lt = text.IndexOf('<', i);
            if (lt < 0)
            {
                AppendText(inlines, text.AsSpan(i), state);
                break;
            }

            // text run before tag
            if (lt > i)
                AppendText(inlines, text.AsSpan(i, lt - i), state);

            var gt = text.IndexOf('>', lt + 1);
            if (gt < 0)
            {
                // malformed tag, render the rest as text
                AppendText(inlines, text.AsSpan(lt), state);
                break;
            }

            var rawTag = text.Substring(lt + 1, gt - lt - 1).Trim();
            i = gt + 1;

            if (rawTag.Length == 0)
                continue;

            // Self-closing forms: <br/>, <icon .../>
            var selfClosing = rawTag.EndsWith("/", StringComparison.Ordinal);
            if (selfClosing) rawTag = rawTag[..^1].TrimEnd();

            var isClose = rawTag.StartsWith("/", StringComparison.Ordinal);
            var tagBody = isClose ? rawTag[1..].TrimStart() : rawTag;
            var (tagName, attrs) = SplitTag(tagBody);
            tagName = tagName.ToLowerInvariant();

            if (isClose)
            {
                // pop until we find matching name
                while (stack.Count > 0)
                {
                    var top = stack.Pop();
                    state = top.Previous;
                    if (string.Equals(top.Name, tagName, StringComparison.OrdinalIgnoreCase))
                        break;
                }
                continue;
            }

            switch (tagName)
            {
                case "br":
                    inlines.Add(new LineBreak());
                    break;

                case "clear":
                    // Visual break in preview.
                    inlines.Add(new LineBreak());
                    inlines.Add(new LineBreak());
                    break;

                case "i":
                    stack.Push(new TagFrame(tagName, state));
                    state = state with { Italic = true };
                    if (selfClosing) { state = stack.Pop().Previous; }
                    break;

                case "strong":
                    stack.Push(new TagFrame(tagName, state));
                    state = state with { Bold = true };
                    if (selfClosing) { state = stack.Pop().Previous; }
                    break;

                case "a":
                    stack.Push(new TagFrame(tagName, state));
                    state = state with { Link = GetAttr(attrs, "href") };
                    if (selfClosing) { state = stack.Pop().Previous; }
                    break;

                case "font":
                    {
                        stack.Push(new TagFrame(tagName, state));

                        var colorStr = GetAttr(attrs, "color");
                        var sizeStr = GetAttr(attrs, "size");
                        var weightStr = GetAttr(attrs, "weight");
                        var opacityStr = GetAttr(attrs, "opacity");

                        var bold = state.Bold;
                        if (!string.IsNullOrWhiteSpace(weightStr))
                            bold = string.Equals(weightStr.Trim(), "bold", StringComparison.OrdinalIgnoreCase);

                        var col = state.Color;
                        if (TryParseHexColor(colorStr, out var parsedCol))
                            col = parsedCol;

                        double? fs = state.FontSize;
                        if (double.TryParse(sizeStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedSize))
                            fs = parsedSize;

                        double? op = state.Opacity;
                        if (double.TryParse(opacityStr, NumberStyles.Float, CultureInfo.InvariantCulture, out var parsedOpacity))
                            op = Math.Clamp(parsedOpacity, 0, 1);

                        state = state with { Bold = bold, Color = col, FontSize = fs, Opacity = op };
                        if (selfClosing) { state = stack.Pop().Previous; }
                        break;
                    }

                case "hk":
                case "hotkey":
                    // We can't resolve key bindings outside the game.
                    // Preview as [HK: code]
                    stack.Push(new TagFrame(tagName, state));
                    state = state with { Monospace = true, HotkeyCode = "" };
                    if (selfClosing) { state = stack.Pop().Previous; }
                    break;

                case "icon":
                    // Placeholder, because rendering SVG icons would require VS assets.
                    var name = GetAttr(attrs, "name") ?? GetAttr(attrs, "path") ?? "icon";
                    AppendPill(inlines, $"[icon: {name}]", state);
                    break;

                case "itemstack":
                    {
                        var type = GetAttr(attrs, "type") ?? "item";
                        var code = GetAttr(attrs, "code") ?? "unknown";
                        AppendPill(inlines, $"[{type}: {code}]", state);
                        break;
                    }

                default:
                    // Unknown tag: ignore (but keep content)
                    if (!selfClosing)
                        stack.Push(new TagFrame(tagName, state));
                    break;
            }
        }

        return inlines;
    }

    private static void AppendText(List<Inline> inlines, ReadOnlySpan<char> span, StyleState state)
    {
        if (span.Length == 0) return;

        // Replace CRLF with LF for predictable LineBreaks
        var normalized = span.ToString().Replace("\r\n", "\n");
        var parts = normalized.Split('\n');
        for (int p = 0; p < parts.Length; p++)
        {
            if (p > 0) inlines.Add(new LineBreak());
            if (parts[p].Length == 0) continue;

            var run = new Run(parts[p]);
            ApplyStyle(run, state);

            // Note: Avalonia's Run (Inline) does not support ToolTip. -wild shrug-
            // If you want link tooltips in the future, the link segment would need to
            // be rendered as an InlineUIContainer containing a Control m'kay.

            inlines.Add(run);
        }
    }

    private static void AppendPill(List<Inline> inlines, string text, StyleState state)
    {
        var run = new Run(text);
        ApplyStyle(run, state with { Monospace = true, Bold = true });
        inlines.Add(run);
    }

    private static void ApplyStyle(Run run, StyleState state)
    {
        run.FontStyle = state.Italic ? FontStyle.Italic : FontStyle.Normal;
        run.FontWeight = state.Bold ? FontWeight.Bold : FontWeight.Normal;

        if (state.Monospace)
            run.FontFamily = new FontFamily("Consolas");

        if (state.FontSize is not null)
            run.FontSize = state.FontSize.Value;

        if (state.Color is not null)
            run.Foreground = new SolidColorBrush(state.Color.Value);

        // Make links look like links.
        if (!string.IsNullOrWhiteSpace(state.Link))
        {
            run.TextDecorations = TextDecorations.Underline;
            if (state.Color is null)
                run.Foreground = Brushes.DodgerBlue;
        }

        // Avalonia Run doesn't expose Opacity. Approximate by reducing foreground alpha.
        if (state.Opacity is not null)
            run.Foreground = WithOpacity(run.Foreground, state.Opacity.Value);
    }

    private static IBrush? WithOpacity(IBrush? brush, double opacity)
    {
        if (brush is null) return null;
        opacity = Math.Clamp(opacity, 0, 1);
        if (brush is SolidColorBrush scb)
        {
            var c = scb.Color;
            var a = (byte)Math.Clamp((int)Math.Round(opacity * 255), 0, 255);
            return new SolidColorBrush(Color.FromArgb(a, c.R, c.G, c.B));
        }
        // If it's not a solid brush (gradient/etc.), leave it alone.
        return brush;
    }

    private static (string name, Dictionary<string, string> attrs) SplitTag(string tagBody)
    {
        // naive attribute parser: key="value" or key=value
        var attrs = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

        var space = tagBody.IndexOf(' ');
        if (space < 0)
            return (tagBody.Trim(), attrs);

        var name = tagBody[..space].Trim();
        var rest = tagBody[(space + 1)..].Trim();

        int i = 0;
        while (i < rest.Length)
        {
            while (i < rest.Length && char.IsWhiteSpace(rest[i])) i++;
            if (i >= rest.Length) break;

            int eq = rest.IndexOf('=', i);
            if (eq < 0) break;

            var key = rest.Substring(i, eq - i).Trim();
            i = eq + 1;

            if (i >= rest.Length) break;

            string value;
            if (rest[i] == '"')
            {
                i++;
                int end = rest.IndexOf('"', i);
                if (end < 0) end = rest.Length;
                value = rest.Substring(i, end - i);
                i = Math.Min(end + 1, rest.Length);
            }
            else
            {
                int end = i;
                while (end < rest.Length && !char.IsWhiteSpace(rest[end])) end++;
                value = rest.Substring(i, end - i);
                i = end;
            }

            if (!string.IsNullOrWhiteSpace(key))
                attrs[key] = value;
        }

        return (name, attrs);
    }

    private static string? GetAttr(Dictionary<string, string> attrs, string key)
        => attrs.TryGetValue(key, out var val) ? val : null;

    public static bool TryParseHexColor(string? hex, out Color color)
    {
        color = default;
        if (string.IsNullOrWhiteSpace(hex)) return false;

        var h = hex.Trim();
        if (h.StartsWith("#", StringComparison.Ordinal)) h = h[1..];

        // Allow RGB or RRGGBB
        if (h.Length == 3)
        {
            var r = new string(h[0], 2);
            var g = new string(h[1], 2);
            var b = new string(h[2], 2);
            h = r + g + b;
        }

        if (h.Length != 6) return false;
        if (!byte.TryParse(h[..2], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var rr)) return false;
        if (!byte.TryParse(h[2..4], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var gg)) return false;
        if (!byte.TryParse(h[4..6], NumberStyles.HexNumber, CultureInfo.InvariantCulture, out var bb)) return false;

        color = Color.FromRgb(rr, gg, bb);
        return true;
    }

    public static string ToHexRgb(Color c)
        => $"#{c.R:X2}{c.G:X2}{c.B:X2}";
}
