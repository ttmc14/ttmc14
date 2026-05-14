using Content.Shared._RMC14.Marines.Skills;
using Content.Shared.Body.Prototypes;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Medical.Defibrillator.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCDefibrillatorComponent : Component
{
    #region Delay

    [DataField]
    public string UsageDelayId = "defib-delay";

    [DataField]
    public TimeSpan UsageDelay = TimeSpan.FromSeconds(5);

    #endregion

    #region Metabolism

    [DataField]
    public ProtoId<MetabolismGroupPrototype> MetabolismId = "Medicine";

    #endregion

    #region Do after

    [DataField]
    public TimeSpan DoAfterBase = TimeSpan.FromSeconds(7);

    [DataField]
    public TimeSpan DoAfterUnskilledPenalty = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan DoAfterSkillReduction = TimeSpan.FromSeconds(1);

    #endregion

    #region Heal

    [DataField]
    public DamageSpecifier HealTypes = new()
    {
        DamageDict =
        {
            { "MCBrute", 1 },
            { "MCBurn", 1 },
            { "MCToxin", 1 },
            { "MCOxygen", 99999 },
            { "MCClone", 1 },
        },
    };

    [DataField]
    public float HealBaseValue = 2f;

    #endregion

    #region Skill

    [DataField]
    public EntProtoId<SkillDefinitionComponent> SkillId = "MCSkillMedical";

    [DataField]
    public float SkillHealMultiplier = 4;

    [DataField]
    public int SkillLevel = 3;

    #endregion

    #region Effects sound

    [DataField]
    public SoundSpecifier? EffectSoundZap = new SoundPathSpecifier("/Audio/Items/Defib/defib_zap.ogg");

    [DataField]
    public SoundSpecifier? EffectSoundFailure = new SoundPathSpecifier("/Audio/Items/Defib/defib_failed.ogg");

    [DataField]
    public SoundSpecifier? EffectSoundSuccess = new SoundPathSpecifier("/Audio/Items/Defib/defib_success.ogg");

    [DataField]
    public SoundSpecifier? EffectSoundReady = new SoundPathSpecifier("/Audio/Items/Defib/defib_ready.ogg");

    [DataField]
    public SoundSpecifier? EffectSoundCharge = new SoundPathSpecifier("/Audio/Items/Defib/defib_charge.ogg");

    [DataField]
    public EntityUid? EffectSoundChargeEntity;

    #endregion
}

[Serializable, NetSerializable]
public sealed partial class MCDefibrillatorApplyDoAfterEvent : SimpleDoAfterEvent;
