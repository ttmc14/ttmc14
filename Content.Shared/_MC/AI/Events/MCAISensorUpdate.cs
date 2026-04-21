using Content.Shared._MC.AI.Modules;

namespace Content.Shared._MC.AI.Events;

[ByRefEvent]
public struct MCAISensorUpdate<T>(T sensor, MCAIMemory memory) where T : MCAISensor<T>
{
    public readonly T Sensor = sensor;
    public readonly MCAIMemory Memory = memory;
}
