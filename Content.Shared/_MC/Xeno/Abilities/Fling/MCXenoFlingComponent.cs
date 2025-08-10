using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Fling;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoFlingComponent : Component
{
    [DataField, AutoNetworkedField]
    public float KnockbackDistance = 4;

    [DataField, AutoNetworkedField]
    public float KnockbackDistanceBig = 3;

    [DataField, AutoNetworkedField]
    public float KnockbackSpeed = 20;

    [DataField, AutoNetworkedField]
    public float EmpowerMultiplier = 2;

    // Effects

    [DataField, AutoNetworkedField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_MC/Effects/alien_claw_block.ogg");
}
