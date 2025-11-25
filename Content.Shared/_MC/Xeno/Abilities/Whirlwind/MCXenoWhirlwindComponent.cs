using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Whirlwind;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCXenoWhirlwindSystem))]
public sealed partial class MCXenoWhirlwindComponent : Component
{
    [DataField, AutoNetworkedField]
    public EntProtoId Fire = "MCTileFireXenoAcid";

    [DataField, AutoNetworkedField]
    public TimeSpan DoAfter;

    [DataField, AutoNetworkedField]
    public TimeSpan BarricadeDuration = TimeSpan.FromSeconds(20);

    [DataField, AutoNetworkedField]
    public int Range = 7;

    [DataField, AutoNetworkedField]
    public int Count = 1;

    [DataField, AutoNetworkedField]
    public FixedPoint2 PlasmaCost = 50;

    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(0.6);

    [DataField, AutoNetworkedField]
    public TimeSpan Cooldown = TimeSpan.FromSeconds(15);

    [DataField, AutoNetworkedField]
    public Angle MaxDeviation = Angle.FromDegrees(0);

    [DataField, AutoNetworkedField]
    public float Speed = 5;

    [DataField, AutoNetworkedField]
    public EntProtoId ProjectileId = "MCXenoProjectileWhirlwind";

    [DataField, AutoNetworkedField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_MC/Effects/Pyrogen/prepare.ogg");
}
