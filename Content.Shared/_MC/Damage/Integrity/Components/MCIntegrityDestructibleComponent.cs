using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Damage.Integrity.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCIntegrityDestructibleComponent : Component
{
    [DataField, AutoNetworkedField]
    public ProtoId<MCIntegrityPrototype> Integrity = "Destruction";
}
