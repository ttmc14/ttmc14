using Robust.Shared.GameStates;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Projectiles;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoSpawnAcidSprayOnTerminatingComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntityCoordinates? Origin;

    [DataField(required: true), AutoNetworkedField]
    public EntProtoId Spawn;

    [DataField, AutoNetworkedField]
    public bool ProjectileAdjust = false;
}
