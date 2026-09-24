using JetBrains.Annotations;

namespace Content.Shared._MC.Engineering.Miners.Events.Modules;

[ByRefEvent]
public struct MCMinerGetProductionTimeEvent(TimeSpan value)
{
    public TimeSpan Value { get; private set; } = value;

    [PublicAPI]
    public void Set(TimeSpan value)
    {
        Value = value;
    }

    [PublicAPI]
    public void Add(TimeSpan value)
    {
        Value += value;
    }

    [PublicAPI]
    public void Remove(TimeSpan value)
    {
        Value -= value;
    }
}
