using Robust.Shared.GameStates;

namespace Content.Shared._MC.ASRS.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCASRSExporterComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Efficiency = 1f;

    [DataField, AutoNetworkedField]
    public LookupFlags IntersectingFlags = LookupFlags.Dynamic | LookupFlags.Sundries;

    [DataField, AutoNetworkedField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(30);

    [ViewVariables, AutoNetworkedField]
    public TimeSpan LastExportTime;
}
