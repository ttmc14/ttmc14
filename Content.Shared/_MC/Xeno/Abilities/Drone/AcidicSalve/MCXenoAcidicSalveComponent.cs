using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Drone.AcidicSalve;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoAcidicSalveComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(1);

    [DataField, AutoNetworkedField]
    public float Range = 1.5f;

    [DataField, AutoNetworkedField]
    public EntProtoId EffectProtoId = "RMCEffectHealHealer";

    [DataField, AutoNetworkedField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_MC/Voice/drool1.ogg");
}
