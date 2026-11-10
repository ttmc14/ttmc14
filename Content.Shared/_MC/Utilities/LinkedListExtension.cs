namespace Content.Shared._MC.Utilities;

public static class LinkedListExtensions
{
    /// <summary>
    /// Returns 1-based position of the first occurrence of <paramref name="value"/>
    /// in the linked list, or -1 if not found.
    /// </summary>
    public static int PositionOf<T>(this LinkedList<T> list, T value)
    {
        var position = 1;
        foreach (var item in list)
        {
            if (EqualityComparer<T>.Default.Equals(item, value))
                return position;

            position++;
        }

        return -1;
    }
}
