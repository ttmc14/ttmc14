using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Sentinel.ToxicStacks;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoToxicStacksOnHitComponent : Component
{
    [DataField, AutoNetworkedField]
    public int Amount;
}
