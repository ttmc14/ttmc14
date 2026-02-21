using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Queen.Screech;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
[Access(typeof(MCXenoScreechSystem))]
public sealed partial class MCXenoScreechComponent : Component
{
    [DataField, AutoNetworkedField]
    public float StunRange = 7;

    [DataField, AutoNetworkedField]
    public EntProtoId EntProtoEffect = "CMEffectScreech";

    [DataField, AutoNetworkedField]
    public SoundSpecifier SoundEffect = new SoundPathSpecifier("/Audio/_RMC14/Xeno/alien_queen_screech.ogg", AudioParams.Default.WithVolume(-7));
}
