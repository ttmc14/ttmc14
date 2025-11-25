using Content.Shared._MC.Beacon.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Beacon.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCBeaconComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<MCBeaconCategoryPrototype> Category;
}
