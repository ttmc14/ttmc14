using Robust.Shared.Prototypes;

namespace Content.Server._MC.Squad;

[RegisterComponent]
public sealed partial class MCSquadSystemDataComponent : Component
{
    [DataField]
    public Dictionary<EntProtoId, EntityUid> Squads = new();
}
