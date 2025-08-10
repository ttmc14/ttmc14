using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.ScatterSpit;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoScatterSpitComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId ProjectileId = "MCXenoProjectileHeavyScatter";

    [DataField, AutoNetworkedField]
    public float Speed = 15;

    [DataField, AutoNetworkedField]
    public int Count = 6;

    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(0.5);

    [DataField, AutoNetworkedField]
    public SoundSpecifier Sound = new SoundCollectionSpecifier("XenoSpitAcid", AudioParams.Default.WithVolume(-10f));

    [DataField, AutoNetworkedField]
    public Angle MaxDeviation = Angle.FromDegrees(4);
}
