using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Widow.Order;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoOrderReceiverComponent : Component
{
    [DataField, AutoNetworkedField]
    public string CurrentOrder = string.Empty;
}
