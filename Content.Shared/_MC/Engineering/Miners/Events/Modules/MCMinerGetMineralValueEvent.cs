namespace Content.Shared._MC.Engineering.Miners.Events.Modules;

[ByRefEvent]
public struct MCMinerGetMineralValueEvent(int value)
{
    public int Value { get; private set; } = value;

    public void Add(int value)
    {
        Value += value;
    }
}
