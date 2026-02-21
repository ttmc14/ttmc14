using Content.Shared._MC.Weapon.Vali.Effects;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.DoAfter;
using Content.Shared.FixedPoint;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Weapon.Vali.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCWeaponValiComponent : Component
{
    #region Actions

    [DataField, AutoNetworkedField]
    public EntProtoId ActionSelectReagentId = "MCActionValiSelectReagent";

    [DataField, AutoNetworkedField]
    public EntityUid? ActionSelectReagent;

    #endregion

    #region Reagent

    [DataField]
    public Dictionary<ProtoId<ReagentPrototype>, MCWeaponValiReagentData> ReagentData = new();

    [DataField, AutoNetworkedField]
    public Dictionary<ProtoId<ReagentPrototype>, FixedPoint2> Reagents = new();

    [DataField, AutoNetworkedField]
    public FixedPoint2 ReagentCapacity = 30;

    [DataField, AutoNetworkedField]
    public ProtoId<ReagentPrototype>? Reagent;

    [DataField, AutoNetworkedField]
    public SpriteSpecifier.Rsi ReagentDefaultIcon;

    [DataField, AutoNetworkedField]
    public TimeSpan ReagentSelectDelay = TimeSpan.FromSeconds(1);

    [ViewVariables]
    public DoAfterId? ReagentSelectDoAfterId;

    [DataField, AutoNetworkedField]
    public TimeSpan ReagentFillDelay = TimeSpan.FromSeconds(2);

    [ViewVariables]
    public DoAfterId? ReagentFillDoAfterId;


    #endregion

    #region Examine

    [DataField, AutoNetworkedField]
    public int ExamineGroupPriority = 10;

    #endregion

    /// <remarks>
    /// Per hit.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public FixedPoint2 ReagentUsage = 5;

    /// <remarks>
    /// For vali module, not for self.
    /// Just data.
    /// </remarks>
    [DataField, AutoNetworkedField]
    public float HarvestAmount = 20;
}

[DataDefinition, Serializable]
public partial struct MCWeaponValiReagentData
{
    [DataField]
    public SpriteSpecifier.Rsi Icon;

    [DataField]
    public List<MCWeaponReagentEffect> Effects = new();
}
