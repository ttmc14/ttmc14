using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Engineering.Vending.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCVendorItemComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId VendorProtoId;
}
