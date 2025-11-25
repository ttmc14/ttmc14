using Content.Shared._RMC14.Line;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Whirlwind;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCXenoWhirlwindSystem))]
public sealed partial class MCXenoWhirlwindSprayingComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId Fire;

    [DataField, AutoNetworkedField]
    public List<LineTile> Spawn = new();

    [DataField, AutoNetworkedField]
    public EntityUid? Blocker;

    [DataField, AutoNetworkedField]
    public EntityUid? Chain;
}
