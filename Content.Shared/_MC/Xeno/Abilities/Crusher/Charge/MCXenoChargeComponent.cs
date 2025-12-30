using Content.Shared.Chat.Prototypes;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Crusher.Charge;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoChargeComponent : Component
{
    [DataField, AutoNetworkedField]
    public DamageSpecifier? Damage;

    [DataField, AutoNetworkedField]
    public DamageSpecifier? StructureDamage;

    [DataField, AutoNetworkedField]
    public float MinimumSteps = 7;

    [DataField, AutoNetworkedField]
    public int MaxStage = 14;

    [DataField, AutoNetworkedField]
    public float StepIncrement = 1;

    [DataField, AutoNetworkedField]
    public float SpeedPerStage = 0.2f;

    [DataField, AutoNetworkedField]
    public FixedPoint2 PlasmaUseMultiplier = 2;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_footstep_charge1.ogg", AudioParams.Default.WithVolume(-4));

    [DataField, AutoNetworkedField]
    public int SoundEvery = 4;

    [DataField, AutoNetworkedField]
    public float MaxDeviation = 1;

    [DataField, AutoNetworkedField]
    public ProtoId<EmotePrototype>? Emote = "XenoRoar";

    [DataField, AutoNetworkedField]
    public TimeSpan? EmoteCooldown = TimeSpan.FromSeconds(20);

    [DataField, AutoNetworkedField]
    public TimeSpan LastMovedGrace = TimeSpan.FromSeconds(0.5);
}
