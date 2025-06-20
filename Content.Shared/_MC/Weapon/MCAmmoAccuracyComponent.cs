using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Weapon;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCAmmoAccuracyComponent : Component
{
    [DataField]
    public string ContainerId = "ballistic-ammo";

    [DataField]
    public List<AmmoAccuracyModifier> Modifiers = new();
}

[DataDefinition]
public sealed partial class AmmoAccuracyModifier
{
    [DataField]
    public EntProtoId Id;

    [DataField]
    public float AccuracyMultiplier = 1;

    [DataField]
    public float ScatterFlat;
}
