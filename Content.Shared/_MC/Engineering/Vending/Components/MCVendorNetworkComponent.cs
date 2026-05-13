using Robust.Shared.GameStates;

namespace Content.Shared._MC.Engineering.Vending.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCVendorNetworkComponent : Component
{
    [DataField(required: true), AutoNetworkedField]
    public string NetworkId = string.Empty;

    [DataField, AutoNetworkedField]
    public Dictionary<string, int> SharedAmounts = new();
}
