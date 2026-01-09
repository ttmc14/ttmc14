using Content.Shared.Chemistry.Reagent;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Projectiles;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoProjectileInjectComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Solution = "chemicals";

    [DataField, AutoNetworkedField]
    public List<ReagentQuantity> Reagents = new();
}
