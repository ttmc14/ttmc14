using Content.Shared.Explosion;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Explosion;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCExplosiveComponent : Component
{
    [DataField]
    public float Power;

    [DataField]
    public float Falloff;

    [DataField]
    public ProtoId<ExplosionPrototype>? ExplosionType;
}
