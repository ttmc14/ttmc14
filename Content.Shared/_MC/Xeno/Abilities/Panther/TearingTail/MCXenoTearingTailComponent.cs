using Content.Shared._MC.CameraShake;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Panther.TearingTail;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCXenoTearingTailComponent : Component
{
    [DataField]
    public float Range = 1.5f;

    [DataField]
    public float PlasmaGain = 25f;

    [DataField]
    public float HealthGain = 70f;

    [DataField]
    public DamageSpecifier TargetDamage = new()
    {
        DamageDict =
        {
            { "MCBrute", 25 },
        },
    };

    [DataField]
    public MCCameraShakeEntry TargetCameraShake = new(2, 1);

    [DataField]
    public float TargetReagentAmount = 5f;

    [DataField]
    public SoundSpecifier EffectSound = new SoundCollectionSpecifier("XenoTailSwipe");

    [DataField]
    public SoundSpecifier EffectHitSound = new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_claw_block.ogg");

    [DataField]
    public EntProtoId EffectHitId = "CMEffectPunch";
}


