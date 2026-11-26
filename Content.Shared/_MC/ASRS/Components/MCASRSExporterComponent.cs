using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.ASRS.Components;

/// <summary>
/// Credits the selected ASRS account for items sold on this export pad.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCASRSExporterComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<MCASRSAccountPrototype> Account = MCASRSAccountPrototype.Default;

    [DataField, AutoNetworkedField]
    public float Efficiency = 1f;

    [DataField, AutoNetworkedField]
    public LookupFlags IntersectingFlags = LookupFlags.Dynamic | LookupFlags.Sundries;

    [DataField, AutoNetworkedField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(30);

    [ViewVariables, AutoNetworkedField]
    public TimeSpan LastExportTime;
}
