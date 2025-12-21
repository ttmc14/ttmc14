using Robust.Shared.Serialization;

namespace Content.Shared._MC.Knockback;

[DataDefinition, Serializable, NetSerializable]
public partial struct MCKnockbackEntry
{
    [DataField]
    public float Distance;

    [DataField]
    public float Speed;

    public MCKnockbackEntry(float distance, float speed)
    {
        Distance = distance;
        Speed = speed;
    }
}
