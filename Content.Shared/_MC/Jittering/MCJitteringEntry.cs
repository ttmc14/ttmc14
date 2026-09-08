using Robust.Shared.Serialization;

namespace Content.Shared._MC.Jittering;

[DataDefinition, Serializable, NetSerializable]
public partial struct MCJitteringEntry
{
    [DataField]
    public TimeSpan Time = TimeSpan.FromSeconds(1);

    [DataField]
    public float Amplitude = 10f;

    [DataField]
    public float Frequency = 4f;

    [DataField]
    public bool Refresh = true;

    [DataField]
    public bool ForceValueChange = false;

    public MCJitteringEntry(TimeSpan time, float amplitude, float frequency)
    {
        Time = time;
        Amplitude = amplitude;
        Frequency = frequency;
    }
}
