using Content.Shared._RMC14.Weapons.Ranged.IFF;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Rules.Utilities;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCRuleRefreshIFFComponent : Component
{
    [ViewVariables, AutoNetworkedField]
    public bool Refreshed;

    [DataField]
    public EntProtoId<IFFFactionComponent> Faction = "FactionMarine";
}
