using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Armor.Abilities.Explode.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCArmorAbilityExplodeComponent : Component
{
    [DataField]
    public EntProtoId ActionProtoId = "MCActionArmorExplode";

    [DataField]
    public EntityUid? Action;

    #region Fire

    [DataField]
    public EntProtoId FireProtoId = "MCFire";

    [DataField]
    public int FireRadius = 5;

    #endregion
}
