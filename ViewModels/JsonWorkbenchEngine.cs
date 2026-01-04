using System;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonWorkbench.ViewModels;

public static class JsonWorkbenchEngine
{
    public static (string newJson, string? error) Apply(string currentJson, JsonInsertOption opt)
    {
        try
        {
            var root = ParseOrCreateRoot(currentJson);

            if (opt.Mode == InsertMode.ReplaceRoot)
            {
                var newRoot = JToken.Parse(opt.TemplateJson);
                return (Pretty(newRoot), null);
            }

            var template = JToken.Parse(opt.TemplateJson);

            switch (opt.Mode)
            {
                case InsertMode.AppendToArray:
                    {
                        var arr = EnsureArray(root, opt.Pointer);
                        arr.Add(template);
                        break;
                    }

                case InsertMode.MergeObject:
                    {
                        var obj = EnsureObject(root, opt.Pointer);
                        if (template is not JObject toMerge)
                            return (currentJson, "Template must be an object for MergeObject.");

                        obj.Merge(toMerge, new JsonMergeSettings
                        {
                            MergeArrayHandling = MergeArrayHandling.Concat,
                            MergeNullValueHandling = MergeNullValueHandling.Ignore
                        });
                        break;
                    }

                case InsertMode.SetIfMissing:
                case InsertMode.Overwrite:
                    {
                        var (parent, leaf) = EnsureParentObject(root, opt.Pointer);
                        if (leaf.Length == 0)
                            return (currentJson, "Invalid pointer (leaf empty).");

                        if (opt.Mode == InsertMode.SetIfMissing && parent[leaf] is not null)
                            return (Pretty(root), null);

                        parent[leaf] = template;
                        break;
                    }
            }

            return (Pretty(root), null);
        }
        catch (JsonReaderException jex)
        {
            return (currentJson, $"JSON parse error: {jex.Message}");
        }
        catch (Exception ex)
        {
            return (currentJson, $"Error: {ex.Message}");
        }
    }

    private static JToken ParseOrCreateRoot(string text)
    {
        var trimmed = (text ?? "").Trim();
        if (string.IsNullOrWhiteSpace(trimmed))
            return new JObject();

        var parsed = JToken.Parse(trimmed);
        return parsed.Type == JTokenType.Object ? parsed : new JObject();
    }

    private static string Pretty(JToken token) =>
        token.ToString(Formatting.Indented);

    // JSON Pointer helpers (RFC6901-ish, with simple object support)
    private static string[] SplitPointer(string pointer)
    {
        if (string.IsNullOrWhiteSpace(pointer) || pointer == "/")
            return Array.Empty<string>();

        return pointer.Split('/')
            .Skip(1)
            .Select(Unescape)
            .Where(s => s is not null)
            .ToArray()!;
    }

    private static string Unescape(string s) =>
        s.Replace("~1", "/").Replace("~0", "~");

    private static (JObject parent, string leaf) EnsureParentObject(JToken root, string pointer)
    {
        var parts = SplitPointer(pointer);
        if (parts.Length == 0) return ((root as JObject) ?? new JObject(), "");

        var parentPath = parts.Take(parts.Length - 1).ToArray();
        var leaf = parts.Last();

        var parentToken = EnsurePath(root, parentPath, wantArray: false);
        if (parentToken is not JObject parentObj)
            throw new InvalidOperationException("Pointer parent is not an object.");

        return (parentObj, leaf);
    }

    private static JObject EnsureObject(JToken root, string pointer)
    {
        var parts = SplitPointer(pointer);
        if (parts.Length == 0)
            return (root as JObject) ?? throw new InvalidOperationException("Root is not an object.");

        var token = EnsurePath(root, parts, wantArray: false);
        if (token is JObject obj) return obj;

        // If something else is there, replace with object
        var (parent, leaf) = EnsureParentObject(root, pointer);
        var newObj = new JObject();
        parent[leaf] = newObj;
        return newObj;
    }

    private static JArray EnsureArray(JToken root, string pointer)
    {
        var parts = SplitPointer(pointer);
        if (parts.Length == 0)
            throw new InvalidOperationException("Pointer must point to a property (not root) for arrays.");

        var token = EnsurePath(root, parts, wantArray: true);
        if (token is JArray arr) return arr;

        var (parent, leaf) = EnsureParentObject(root, pointer);
        var newArr = new JArray();
        parent[leaf] = newArr;
        return newArr;
    }

    private static JToken EnsurePath(JToken root, string[] parts, bool wantArray)
    {
        var cur = root;

        for (int i = 0; i < parts.Length; i++)
        {
            var key = parts[i];

            if (cur is not JObject obj)
                throw new InvalidOperationException("Encountered non-object while walking pointer.");

            var isLeaf = i == parts.Length - 1;

            if (obj[key] is null)
            {
                obj[key] = isLeaf
                    ? (wantArray ? new JArray() : new JObject())
                    : new JObject();
            }

            cur = obj[key]!;
        }

        return cur;
    }
}
