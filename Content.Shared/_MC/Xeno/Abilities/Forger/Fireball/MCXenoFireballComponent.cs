using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Forger.Fireball;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoFireballComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(0.6);

    [DataField, AutoNetworkedField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(15);

    [DataField, AutoNetworkedField]
    public Angle MaxDeviation = Angle.FromDegrees(0);

    [DataField, AutoNetworkedField]
    public int Range = 8;

    [DataField, AutoNetworkedField]
    public float Speed = 15;

    [DataField, AutoNetworkedField]
    public EntProtoId ProjectileId = "MCXenoProjectileFireball";

    [DataField, AutoNetworkedField]
    public SoundSpecifier SoundPrepare = new SoundPathSpecifier("/Audio/_MC/Effects/Pyrogen/wind.ogg");

    [DataField, AutoNetworkedField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_MC/Effects/Pyrogen/fireball.ogg");
}
