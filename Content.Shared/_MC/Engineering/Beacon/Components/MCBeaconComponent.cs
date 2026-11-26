using Content.Shared._MC.Engineering.Beacon.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Engineering.Beacon.Components;

/// <summary>
/// Identifies a named destination for systems that accept this beacon category.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCBeaconComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Name = string.Empty;

    [DataField, AutoNetworkedField]
    public ProtoId<MCBeaconCategoryPrototype> Category;
}
