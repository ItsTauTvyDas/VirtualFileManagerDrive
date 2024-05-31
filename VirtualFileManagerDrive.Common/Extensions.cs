namespace VirtualFileManagerDrive.Common;

public static class Extensions
{
    public static TV? GetValueOrDefault<TK, TV>(this Dictionary<TK, TV?> dictionary, TK key, TV? defaultValue) where TK : notnull
    {
        if (dictionary.TryGetValue(key, out var val))
        {
            return val ?? defaultValue;
        }

        return defaultValue;
    }

    public static void AddToList<TK, TV>(this Dictionary<TK, IList<TV>> dictionary, TK key, TV value) where TK : notnull
    {
        if (dictionary.TryGetValue(key, out var list))
        {
            list.Add(value);
            return;
        }
        dictionary.Add(key, new List<TV> { value });
    }
}