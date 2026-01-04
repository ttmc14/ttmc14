using Content.Shared._MC.CameraShake;
using Content.Shared._MC.Knockback;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Crusher.Stomp;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoStompComponent : Component
{
    /// <summary>
    /// Ability range
    /// </summary>
    [DataField, AutoNetworkedField]
    public float Distance = 1.5f;

    /// <summary>
    /// The distance that is considered sufficient to enhance the effects
    /// </summary>
    [DataField, AutoNetworkedField]
    public float ExtraAffectDistance = 1.15f;

    /// <summary>
    /// Damage dealt on an entity is calculated using the
    /// following formula: Damage / Math.Max(1, distance + 1)
    /// </summary>
    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage;

    [DataField, AutoNetworkedField]
    public TimeSpan Paralyze = TimeSpan.FromSeconds(0.5f);

    [DataField, AutoNetworkedField]
    public TimeSpan ExtraParalyze = TimeSpan.FromSeconds(3f);

    [DataField, AutoNetworkedField]
    public MCCameraShakeEntry CameraShakeEntry = new(2, 2);

    [DataField, AutoNetworkedField]
    public MCCameraShakeEntry ExtraCameraShakeEntry = new(3, 3);

    [DataField, AutoNetworkedField]
    public MCKnockbackEntry ThrowEntry = new(1, 10);

    #region Effects

    [DataField, AutoNetworkedField]
    public EntProtoId EffectProtoId = "CMEffectSelfStomp";

    [DataField, AutoNetworkedField]
    public SoundSpecifier EffectSound = new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_footstep_charge1.ogg");

    #endregion
}
