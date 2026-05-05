using Content.Shared._MC.Knockback;
using Content.Shared.Damage;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Chimera.WarpBlast;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCXenoWarpBlastComponent : Component
{
    [DataField]
    public DamageSpecifier Damage = new()
    {
        DamageDict =
        {
            { "MCBrute", 30f },
        },
    };

    [DataField]
    public float DamageStamina = 60f;

    [DataField]
    public float Range = 2.5f;

    [DataField]
    public TimeSpan ParalyzeDuration = TimeSpan.FromSeconds(0.5);

    [DataField]
    public MCKnockbackEntry Knockback = new(2, 25);

    [DataField]
    public SoundSpecifier? EffectSound = new SoundPathSpecifier("/Audio/_RMC14/Effects/bamf.ogg");
}
