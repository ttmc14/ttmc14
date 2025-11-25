using Content.Shared.Chemistry.Reagent;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Weapon.Vali;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCWeaponValiComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId ActionSelectReagentId = "MCActionValiSelectReagent";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionSelectReagent;

    [DataField, AutoNetworkedField]
    public ProtoId<ReagentPrototype>? SelectedReagent;

    [DataField, AutoNetworkedField]
    public List<ProtoId<ReagentPrototype>> AllowedReagents = new();

    [DataField, AutoNetworkedField]
    public SpriteSpecifier.Rsi ReagentEmptyIcon;

    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<ReagentPrototype>, SpriteSpecifier.Rsi> ReagentIcons = new();

    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> Reagents = new();

    [DataField, AutoNetworkedField]
    public FixedPoint2 ReagentCapacity = 30;

    [DataField, AutoNetworkedField]
    public FixedPoint2 ReagentUsage = 5;
}
