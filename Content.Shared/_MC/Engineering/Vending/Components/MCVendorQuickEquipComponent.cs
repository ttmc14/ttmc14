using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Engineering.Vending.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCVendorQuickEquipComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<EntProtoId> Vendors = new();
}
