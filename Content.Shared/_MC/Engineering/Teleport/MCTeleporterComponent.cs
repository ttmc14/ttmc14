using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Engineering.Teleport;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCTeleporterComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan TeleportNext;

    [DataField]
    public TimeSpan TeleportCooldown = TimeSpan.FromSeconds(2);

    [DataField]
    public SoundSpecifier EffectSoundTeleport = new SoundPathSpecifier("/Audio/_MC/Effects/phasein.ogg");

    [DataField]
    public SoundSpecifier EffectSoundFail = new SoundPathSpecifier("/Audio/_MC/Effects/Machines/buzz-two.ogg");
}
