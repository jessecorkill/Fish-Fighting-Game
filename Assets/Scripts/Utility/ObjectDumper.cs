using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

#if UNITY_5_3_OR_NEWER
using UnityEngine;
#endif

public static class ObjectDumper
{
    public sealed class Options
    {
        public int MaxDepth { get; set; } = 6;
        public int MaxItemsPerEnumerable { get; set; } = 100;
        public bool IncludePrivateMembers { get; set; } = false;
        public bool ShowTypeNames { get; set; } = true;
        public bool SortMembers { get; set; } = true;
        public string Indent { get; set; } = "  ";
    }

    public static string Dump(object obj, Options options = null)
    {
        options ??= new Options();
        var visited = new HashSet<object>(ReferenceEqualityComparer.Instance);
        return DumpInternal(obj, 0, options, visited);
    }

    private static string DumpInternal(object obj, int depth, Options opt, HashSet<object> visited)
    {
        if (depth > opt.MaxDepth)
            return "...";

        if (obj == null)
            return "null";

        // Primitive-ish types
        var type = obj.GetType();
        if (IsSimple(type))
            return FormatSimple(obj, type, opt);

        // Handle UnityEngine.Object for nicer names
#if UNITY_5_3_OR_NEWER
        if (obj is UnityEngine.Object uo)
        {
            var tn = opt.ShowTypeNames ? $"<{uo.GetType().Name}>" : "";
            var name = string.IsNullOrEmpty(uo.name) ? "(no-name)" : uo.name;
            return $"{name}{tn}";
        }
#endif

        // Cycle detection (for reference types only)
        if (!type.IsValueType)
        {
            if (visited.Contains(obj))
                return $"↻(already seen {TypeName(type, opt)})";
            visited.Add(obj);
        }

        // IDictionary
        if (obj is IDictionary dict)
            return DumpDictionary(dict, depth, opt, visited);

        // IEnumerable (but not string, handled above)
        if (obj is IEnumerable seq)
            return DumpEnumerable(seq, depth, opt, visited, type);

        // Complex object: reflect fields/properties
        return DumpObjectMembers(obj, depth, opt, visited);
    }

    private static string DumpDictionary(IDictionary dict, int depth, Options opt, HashSet<object> visited)
    {
        var items = new List<string>();
        int count = 0;
        foreach (DictionaryEntry de in dict)
        {
            if (count++ >= opt.MaxItemsPerEnumerable)
            {
                items.Add("…");
                break;
            }
            string k = DumpInternal(de.Key, depth + 1, opt, visited);
            string v = DumpInternal(de.Value, depth + 1, opt, visited);
            items.Add($"{Indent(depth + 1, opt)}{k}: {v}");
        }
        var header = opt.ShowTypeNames ? $"<{dict.GetType().Name}>" : "";
        return $"{{{header}\n{string.Join("\n", items)}\n{Indent(depth, opt)}}}";
    }

    private static string DumpEnumerable(IEnumerable seq, int depth, Options opt, HashSet<object> visited, Type concreteType)
    {
        var items = new List<string>();
        int i = 0;
        foreach (var item in seq)
        {
            if (i++ >= opt.MaxItemsPerEnumerable)
            {
                items.Add($"{Indent(depth + 1, opt)}…");
                break;
            }
            items.Add($"{Indent(depth + 1, opt)}- {DumpInternal(item, depth + 1, opt, visited)}");
        }
        var header = opt.ShowTypeNames ? $"<{TypeName(concreteType, opt)}>" : "";
        return $"[{header}\n{string.Join("\n", items)}\n{Indent(depth, opt)}]";
    }

    private static string DumpObjectMembers(object obj, int depth, Options opt, HashSet<object> visited)
    {
        var type = obj.GetType();
        var header = opt.ShowTypeNames ? $" {TypeName(type, opt)}" : "";
        var lines = new List<string>();

        // Fields
        var fieldFlags = BindingFlags.Instance | BindingFlags.Public;
        if (opt.IncludePrivateMembers) fieldFlags |= BindingFlags.NonPublic;

        var fields = type.GetFields(fieldFlags);
        if (opt.SortMembers) fields = fields.OrderBy(f => f.Name).ToArray();

        foreach (var f in fields)
        {
            try
            {
                var val = f.GetValue(obj);
                lines.Add($"{Indent(depth + 1, opt)}{f.Name}: {DumpInternal(val, depth + 1, opt, visited)}");
            }
            catch (Exception e)
            {
                lines.Add($"{Indent(depth + 1, opt)}{f.Name}: <error: {e.GetType().Name}>");
            }
        }

        // Properties (skip indexers)
        var propFlags = BindingFlags.Instance | BindingFlags.Public;
        if (opt.IncludePrivateMembers) propFlags |= BindingFlags.NonPublic;

        var props = type.GetProperties(propFlags)
                        .Where(p => p.GetIndexParameters().Length == 0 && p.CanRead);
        if (opt.SortMembers) props = props.OrderBy(p => p.Name);

        foreach (var p in props)
        {
            try
            {
                object val = null;
                // Some Unity properties throw in edit/runtime contexts; guard them
                val = p.GetValue(obj, null);
                lines.Add($"{Indent(depth + 1, opt)}{p.Name}: {DumpInternal(val, depth + 1, opt, visited)}");
            }
            catch (Exception e)
            {
                lines.Add($"{Indent(depth + 1, opt)}{p.Name}: <error: {e.GetType().Name}>");
            }
        }

        return $"{{{header}\n{string.Join("\n", lines)}\n{Indent(depth, opt)}}}";
    }

    private static string FormatSimple(object obj, Type type, Options opt)
    {
        if (obj is string s)
            return $"\"{s}\"";
        if (obj is char c)
            return $"'{c}'";
        if (obj is DateTime dt)
            return dt.ToString("o");
        if (obj is DateTimeOffset dto)
            return dto.ToString("o");
        if (obj is TimeSpan ts)
            return ts.ToString();

        // Enums and numerics etc.
        return obj.ToString();
    }

    private static bool IsSimple(Type t)
    {
        if (t.IsPrimitive || t.IsEnum)
            return true;

        return t == typeof(string)
            || t == typeof(decimal)
            || t == typeof(DateTime)
            || t == typeof(DateTimeOffset)
            || t == typeof(TimeSpan)
            || t == typeof(Guid);
    }

    private static string TypeName(Type t, Options opt)
    {
        if (!opt.ShowTypeNames) return "";
        if (!t.IsGenericType) return t.Name;
        // Pretty-print generics like List<int> instead of List`1
        var genericTypeName = t.Name.Substring(0, t.Name.IndexOf('`'));
        var args = string.Join(", ", t.GetGenericArguments().Select(a => a.Name));
        return $"{genericTypeName}<{args}>";
    }

    private static string Indent(int depth, Options opt) => new string(' ', depth * opt.Indent.Length);
}

/// <summary>
/// Reference equality comparer for cycle detection
/// </summary>
public sealed class ReferenceEqualityComparer : IEqualityComparer<object>
{
    public static readonly ReferenceEqualityComparer Instance = new ReferenceEqualityComparer();
    private ReferenceEqualityComparer() { }
    public new bool Equals(object x, object y) => ReferenceEquals(x, y);
    public int GetHashCode(object obj) => System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(obj);
}
