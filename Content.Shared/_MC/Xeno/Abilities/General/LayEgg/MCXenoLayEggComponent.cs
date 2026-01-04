using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.General.LayEgg;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoLayEggComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId ProtoId = "XenoEgg";

    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(2.5f);

    [DataField, AutoNetworkedField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/Effects/Fluids/splat.ogg");
}
