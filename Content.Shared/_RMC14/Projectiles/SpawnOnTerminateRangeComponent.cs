using Content.Shared.Popups;
using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._RMC14.Projectiles;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(RMCProjectileSystem))]
public sealed partial class SpawnOnTerminateRangeComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityCoordinates? Origin;

    [DataField(required: true), AutoNetworkedField]
    public EntProtoId Spawn;

    [DataField, AutoNetworkedField]
    public float Range = 3f;

    [DataField, AutoNetworkedField]
    public float PositionXOne = 1f;

    [DataField, AutoNetworkedField]
    public float PositionXTwo = -1f;

    [DataField, AutoNetworkedField]
    public float PositionYOne = 1f;

    [DataField, AutoNetworkedField]
    public float PositionYTwo = -1f;

    [DataField, AutoNetworkedField]
    public bool ProjectileAdjust = true;
}
