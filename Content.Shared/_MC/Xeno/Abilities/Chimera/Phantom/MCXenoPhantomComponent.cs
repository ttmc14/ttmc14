using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Chimera.Phantom;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCXenoPhantomComponent : Component
{
    [DataField]
    public EntProtoId EntProtoId = "MCXenoChimeraClone";

    [DataField]
    public TimeSpan InvisibleDuration = TimeSpan.FromSeconds(5);

    [DataField]
    public SoundSpecifier? EffectSound = new SoundPathSpecifier("/Audio/_MC/Effects/magic.ogg");
}
