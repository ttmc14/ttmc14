using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Chimera.BodySwap;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCXenoBodySwapComponent : Component
{
    [DataField]
    public float Range = 9f;

    [DataField]
    public SoundSpecifier? EffectSound = new SoundPathSpecifier("/Audio/_MC/Effects/emp_pulse.ogg");

}
