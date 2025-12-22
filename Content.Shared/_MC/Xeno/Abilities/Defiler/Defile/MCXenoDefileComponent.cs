using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Defiler.Defile;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoDefileComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Solution = "chemicals";

    [DataField, AutoNetworkedField]
    public TimeSpan FailUseCooldown = TimeSpan.FromSeconds(5);

    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(0.5);

    [DataField, AutoNetworkedField]
    public EntProtoId SmokeId = "MCSmokeXenoSanguinal";

    [DataField, AutoNetworkedField]
    public SoundSpecifier? SmokeEffectSound = new SoundPathSpecifier("/Audio/_MC/Effects/Smoke/smoke.ogg");

    [DataField, AutoNetworkedField]
    public float Range = 1.5f;
}
