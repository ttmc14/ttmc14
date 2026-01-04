using Content.Shared.Actions;
using Content.Shared.Chat.Prototypes;
using Robust.Shared.Audio;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Bull.HeadbuttCharge;

public sealed partial class MCXenoHeadbuttChargeActionEvent : InstantActionEvent
{
    [DataField]
    public bool Collide = true;

    [DataField]
    public float Knockback;

    [DataField]
    public float KnockbackSpeed;

    [DataField]
    public TimeSpan Paralyze;

    [DataField]
    public TimeSpan Duration = TimeSpan.FromSeconds(2);

    [DataField]
    public float DamageMultiplier;

    [DataField]
    public float SpeedMultiplier = 1.75f;

    [DataField]
    public EntProtoId? TurfSpawnEntityId;

    [DataField]
    public ProtoId<EmotePrototype> ActivationEmote = "XenoRoar";

    [DataField]
    public SoundSpecifier? HitSound;

    [DataField]
    public SoundSpecifier? FootstepSound;
}
