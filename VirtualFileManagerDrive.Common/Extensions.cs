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

    public static void Add<TK, TV>(this Dictionary<TK, TV?> dictionary, TK key, TV? value) where TK : notnull
    {
        dictionary.Add(key, value);
    }
}