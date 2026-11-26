using Content.Shared.Actions;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Armor.Modules.Features.Antenna;

/// <summary>
/// Grants a supply beacon action while this module is installed in a worn helmet.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCModuleAntennaComponent : Component
{
    /// <summary>
    /// Action granted to the wearer.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId Action = "MCActionModuleAntenna";

    /// <summary>
    /// Action instance currently granted to the wearer.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? ActionEntity;

    /// <summary>
    /// Temporary beacon at the tile registered with ASRS.
    /// </summary>
    [ViewVariables, AutoNetworkedField]
    public EntityUid? Signal;
}

public sealed partial class MCModuleAntennaActionEvent : InstantActionEvent;
