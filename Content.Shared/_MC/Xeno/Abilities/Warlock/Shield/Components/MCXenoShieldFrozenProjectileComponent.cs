using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.Shield.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoShieldFrozenProjectileComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public Vector2 LinerVelocity;

    [ViewVariables, AutoNetworkedField]
    public float AngularVelocity;

    [ViewVariables, AutoNetworkedField]
    public BodyStatus BodyStatus;

    [ViewVariables, AutoNetworkedField]
    public BodyType BodyType;

    [ViewVariables, AutoNetworkedField]
    public bool CanCollide;

    [ViewVariables, AutoNetworkedField]
    public bool DeleteOnCollide;

    [ViewVariables, AutoNetworkedField]
    public bool FixedDistance;

    [ViewVariables, AutoNetworkedField]
    public bool FixedDistanceArcProj;

    [ViewVariables, AutoNetworkedField]
    public TimeSpan FixedDistanceRemaining;

    [ViewVariables, AutoNetworkedField]
    public bool ProjectileSpent;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? Shooter;

    [ViewVariables, AutoNetworkedField]
    public EntityUid? Weapon;
}
