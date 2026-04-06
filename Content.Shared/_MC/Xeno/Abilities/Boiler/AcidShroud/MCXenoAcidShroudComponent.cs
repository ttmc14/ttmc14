using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Boiler.AcidShroud;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoAcidShroudComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan AdjustDelay = TimeSpan.FromSeconds(30);

    [DataField, AutoNetworkedField]
    public int MinRange = 2;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? EffectSound = new SoundPathSpecifier("/Audio/_MC/Effects/Smoke/smoke.ogg");
}
