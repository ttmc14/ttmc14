using Content.Shared._MC.Damage.Integrity.Systems;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Damage.Integrity.Components;

[Access([typeof(MCIntegritySystem)], Other = AccessPermissions.Read)]
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCIntegrityComponent : Component
{
    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<MCIntegrityPrototype>, FixedPoint2> Thresholds = new();
}
