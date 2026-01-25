using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Runner.MelterShroud;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoMelterShroudComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId ShroudId;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? EffectSound = new SoundPathSpecifier("/Audio/_MC/Effects/Smoke/smoke.ogg");
}
