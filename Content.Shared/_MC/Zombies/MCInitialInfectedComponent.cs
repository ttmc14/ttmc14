using Content.Shared.StatusIcon;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Zombies;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCInitialInfectedComponent : Component
{
    [DataField]
    public ProtoId<FactionIconPrototype> StatusIcon = "InitialInfectedFaction";
}
