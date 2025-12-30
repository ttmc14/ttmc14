using Robust.Shared.Serialization;

namespace Content.Shared._MC.CameraShake;

[DataDefinition, Serializable, NetSerializable]
public partial struct MCCameraShakeEntry
{
    [DataField]
    public int Shakes;

    [DataField]
    public int Strength;

    [DataField]
    public TimeSpan? Spacing;

    public MCCameraShakeEntry(int shakes, int strength, TimeSpan? spacing = null)
    {
        Shakes = shakes;
        Strength = strength;
        Spacing = spacing;
    }
}
