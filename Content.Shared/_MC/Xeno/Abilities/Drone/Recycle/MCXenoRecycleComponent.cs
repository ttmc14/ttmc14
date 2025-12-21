using Robust.Shared.Audio;

namespace Content.Shared._MC.Xeno.Abilities.Drone.Recycle;

[RegisterComponent, AutoGenerateComponentState]
[Access(typeof(MCXenoRecycleSystem))]
public sealed partial class MCXenoRecycleComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(7);

    [DataField, AutoNetworkedField]
    public SoundSpecifier EffectSound = new SoundPathSpecifier("/Audio/_MC/Effects/recycler.ogg", AudioParams.Default.WithVolume(-11));
}
