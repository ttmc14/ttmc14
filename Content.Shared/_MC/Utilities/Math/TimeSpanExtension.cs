namespace Content.Shared._MC.Utilities.Math;

public static class TimeSpanExtension
{
    public static TimeSpan EnsureNonNegative(this TimeSpan timeSpan)
    {
        return timeSpan < TimeSpan.Zero ? TimeSpan.Zero : timeSpan;
    }

    public static TimeSpan Max(this TimeSpan timeSpan, TimeSpan other)
    {
        return timeSpan > other ? timeSpan : other;
    }

    public static TimeSpan Min(this TimeSpan timeSpan, TimeSpan other)
    {
        return timeSpan < other ? timeSpan : other;
    }
}
