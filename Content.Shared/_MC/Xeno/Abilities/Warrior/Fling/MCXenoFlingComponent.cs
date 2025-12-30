using Content.Shared._MC.CameraShake;
using Content.Shared._MC.Knockback;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Warrior.Fling;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCXenoFlingSystem))]
public sealed partial class MCXenoFlingComponent : Component
{
    [DataField, AutoNetworkedField]
    public float EmpowerMultiplier = 2;

    [DataField, AutoNetworkedField]
    public MCCameraShakeEntry CameraShakeEntry = new(1, 1);

    [DataField, AutoNetworkedField]
    public MCKnockbackEntry KnockbackEntry = new(4, 20);

    [DataField, AutoNetworkedField]
    public MCKnockbackEntry KnockbackBigEntry = new(4, 20);

    [DataField, AutoNetworkedField]
    public SoundSpecifier EffectsSound = new SoundPathSpecifier("/Audio/_MC/Effects/alien_claw_block.ogg");
}
