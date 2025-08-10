using Content.Shared._MC.Xeno.Blessings.Prototypes;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Blessings;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoBlessingsComponent : Component
{
    [DataField, AutoNetworkedField]
    public List<ProtoId<MCXenoBlessingsEntryPrototype>> Entries = new();
}
