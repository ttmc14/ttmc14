using Content.Shared.Explosion;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Armor.Abilities.Explode.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCArmorAbilityExplodeComponent : Component
{
    [DataField]
    public EntProtoId ActionProtoId = "MCFire";

    [DataField]
    public EntityUid? Action;

    #region Explosion

    [DataField]
    public ProtoId<ExplosionPrototype> ExplosionType = "MC";

    [DataField]
    public float MaxIntensity = 275;

    [DataField]
    public float TotalIntensity = 275;

    [DataField]
    public float IntensitySlope = 65;

    #endregion

    #region Fire

    [DataField]
    public EntProtoId FireProtoId = "MCFire";

    [DataField]
    public int FireRadius = 5;

    #endregion
}
