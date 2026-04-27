using Content.Shared.Damage;
using JetBrains.Annotations;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Xeno.Constructions.AcidWell;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoAcidWellComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan TimeAutoChargeNext = TimeSpan.Zero;

    [DataField, AutoNetworkedField]
    public TimeSpan TimeAutoChargeDelay = TimeSpan.FromSeconds(45);

    [DataField, AutoNetworkedField]
    public int ChargesMax = 5;

    [DataField, AutoNetworkedField]
    public int ChargesAutoMax = 3;

    [DataField, AutoNetworkedField]
    public int Charges = 1;

    [DataField, AutoNetworkedField]
    public EntProtoId SmokeProtoId = "MCSmokeXenoAcidExtuingishing";

    [DataField, AutoNetworkedField]
    public DamageSpecifier StepDamage = new()
    {
        DamageDict =
        {
            { "MCBurn", 20 },
        },
    };
}

[Serializable, NetSerializable, UsedImplicitly]
public enum MCXenoAcidWellLayers
{
    Fill,
}

[Serializable, NetSerializable]
public enum MCXenoAcidWellVisuals
{
    Fill,
}
