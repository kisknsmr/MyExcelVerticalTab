using System;
using System.Collections.Concurrent;
using System.Runtime.InteropServices;

namespace ExcelVerticalTab;

public static class Helper
{
    public static EventArgs<T> CreateEventArgs<T>(T value) => new(value);

    public static TValue? GetValueOrDefault<TKey, TValue>(this ConcurrentDictionary<TKey, TValue> dict, TKey key, TValue? defaultValue = default)
        where TKey : notnull
    {
        return dict.TryGetValue(key, out var value) ? value : defaultValue;
    }

    #region NativeCall
    [DllImport("user32.dll", SetLastError = true)]
    public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string? lpszClass, string? lpszWindow);

    [DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    public static extern uint RegisterWindowMessage(string lpString);
    #endregion
}
