using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.Shield.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCXenoShieldComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId ShieldEntProtoId = "MCXenoShield";

    [DataField, AutoNetworkedField]
    public SoundSpecifier? EffectSoundAction = new SoundPathSpecifier("/Audio/_MC/Effects/magic.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier? EffectSoundEnd = new SoundPathSpecifier("/Audio/_MC/Effects/roar_warlock.ogg");
}
