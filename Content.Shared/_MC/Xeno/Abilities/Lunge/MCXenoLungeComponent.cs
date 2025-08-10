using System.Numerics;
using Robust.Shared.GameStates;
using Robust.Shared.Map;

namespace Content.Shared._MC.Xeno.Abilities.Lunge;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoLungeComponent : Component
{
    [DataField, AutoNetworkedField]
    public float KnockbackDistance = 4;

    [DataField, AutoNetworkedField]
    public float KnockbackSpeed = 20;

    [DataField, AutoNetworkedField]
    public TimeSpan StunTime = TimeSpan.FromSeconds(4);

    [DataField, AutoNetworkedField]
    public Vector2? Charge;

    [DataField, AutoNetworkedField]
    public EntityUid? Target;

    [DataField, AutoNetworkedField]
    public MapCoordinates? Origin;
}
