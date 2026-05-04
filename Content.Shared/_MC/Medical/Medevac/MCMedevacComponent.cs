using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Medical.Medevac;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
public sealed partial class MCMedevacComponent : Component
{
    [DataField, AutoNetworkedField]
    public bool Active;

    [DataField, AutoNetworkedField]
    public TimeSpan ActiveNext;

    [DataField, AutoNetworkedField]
    public TimeSpan EvacNext;

    [DataField, AutoNetworkedField]
    public TimeSpan InteractNext;

    [DataField]
    public TimeSpan ActiveTime = TimeSpan.FromSeconds(5);

    [DataField]
    public TimeSpan EvacCooldown = TimeSpan.FromSeconds(150);

    [DataField]
    public TimeSpan InteractCooldown = TimeSpan.FromSeconds(1);

    [DataField]
    public SoundSpecifier TeleportSound = new SoundPathSpecifier("/Audio/_MC/Effects/phasein.ogg");

    [DataField]
    public SoundSpecifier ActivateSound = new SoundPathSpecifier("/Audio/_MC/Effects/Machines/powerup.ogg");

    [DataField]
    public SoundSpecifier FailSound = new SoundPathSpecifier("/Audio/_MC/Effects/Machines/buzz-two.ogg");
}
