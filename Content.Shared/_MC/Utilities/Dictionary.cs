namespace Content.Shared._MC.Utilities;

public static class Dictionary
{
    public static void MergeSumInPlace(
        this Dictionary<int, int> target,
        Dictionary<int, int> source)
    {
        foreach (var kv in source)
        {
            if (target.TryGetValue(kv.Key, out var existing))
            {
                target[kv.Key] = existing + kv.Value;
                continue;
            }

            target[kv.Key] = kv.Value;
        }
    }

    public static void Fill(this Dictionary<int, int> target, int min, int max, int value)
    {
        for (var i = min; i < max; i++)
        {
            target[i] = value;
        }
    }
}
