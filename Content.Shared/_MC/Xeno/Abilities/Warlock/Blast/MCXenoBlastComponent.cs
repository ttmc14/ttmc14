using Content.Shared._MC.Knockback;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.Blast;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoBlastComponent : Component
{
    [DataField, AutoNetworkedField]
    public float Range = 7;

    [DataField, AutoNetworkedField]
    public float EffectRange = 2.5f;

    [DataField, AutoNetworkedField]
    public TimeSpan SlowdownDuration = TimeSpan.FromSeconds(1f);

    [DataField, AutoNetworkedField]
    public MCKnockbackEntry KnockbackEntry = new(3, 25);

    [DataField, AutoNetworkedField]
    public DamageSpecifier Damage = new()
    {
        DamageDict = new()
        {
            { "MCBurn", 45 },
        },
    };

    [DataField, AutoNetworkedField]
    public EntProtoId EffectId = "MCEffectPsychicBlast";

    [DataField, AutoNetworkedField]
    public EntProtoId RayEffectId = "MCEffectPsychicBlastRay";

    [DataField, AutoNetworkedField]
    public int ArmorPiercing = 10;

    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(0.75f);

    [DataField, AutoNetworkedField]
    public SoundSpecifier? EffectSound = new SoundPathSpecifier("/Audio/_MC/Effects/volkite_4.ogg");
}
