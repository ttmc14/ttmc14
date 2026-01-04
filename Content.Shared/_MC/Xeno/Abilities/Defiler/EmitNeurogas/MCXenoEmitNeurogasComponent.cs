using Robust.Shared.Audio;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Xeno.Abilities.Defiler.EmitNeurogas;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoEmitNeurogasComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(0.5f);

    [DataField, AutoNetworkedField]
    public TimeSpan Duration = TimeSpan.FromSeconds(1);

    [DataField, AutoNetworkedField]
    public int Activations = 3;

    [DataField, AutoNetworkedField]
    public int Range = 2;

    [DataField, AutoNetworkedField]
    public SoundSpecifier? Sound = new SoundPathSpecifier("/Audio/_MC/Effects/Smoke/smoke.ogg");
}
