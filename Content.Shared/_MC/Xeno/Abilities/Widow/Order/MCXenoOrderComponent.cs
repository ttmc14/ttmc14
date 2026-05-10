using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Widow.Order;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoOrderComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Order = string.Empty;
}
