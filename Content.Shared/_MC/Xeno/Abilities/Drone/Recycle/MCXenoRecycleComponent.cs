using Content.Shared._MC.Jittering;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Drone.Recycle;

[RegisterComponent, NetworkedComponent]
[Access(typeof(MCXenoRecycleSystem))]
public sealed partial class MCXenoRecycleComponent : Component
{
    [DataField]
    public TimeSpan Delay = TimeSpan.FromSeconds(7);

    [DataField]
    public MCJitteringEntry EffectJitteringTarget = new(TimeSpan.FromSeconds(3), 10f, 5f);

    [DataField]
    public SoundSpecifier EffectSoundProcessStart = new SoundPathSpecifier("/Audio/_MC/Effects/nightfall.ogg");

    [DataField]
    public SoundSpecifier EffectSoundProcessEnd = new SoundPathSpecifier("/Audio/_MC/Effects/recycler.ogg", AudioParams.Default);
}
