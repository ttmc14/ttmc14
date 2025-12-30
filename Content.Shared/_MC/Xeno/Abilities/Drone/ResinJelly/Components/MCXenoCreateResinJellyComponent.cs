using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Drone.ResinJelly.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCXenoCreateResinJellyComponent : Component
{
    [DataField]
    public EntProtoId ProtoId = "MCXenoResinJelly";
}
