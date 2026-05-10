using Content.Shared.Actions;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.General.SpitOnce;

public sealed partial class MCXenoSpitOnceActionEvent : WorldTargetActionEvent
{
    [DataField, AutoNetworkedField]
    public int? Range = 8;

    [DataField, AutoNetworkedField]
    public float Speed = 15;

    [DataField, AutoNetworkedField]
    public EntProtoId ProjectileId = "MCXenoProjectileFireball";
}
