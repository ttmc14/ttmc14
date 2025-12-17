using Content.Shared.FixedPoint;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Pincushion;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class MCXenoPincushionComponent : Component
{
    [DataField, AutoNetworkedField]
    public TimeSpan Delay = TimeSpan.FromSeconds(0.3);

    [DataField, AutoNetworkedField]
    public Angle MaxDeviation = Angle.FromDegrees(0);

    [DataField, AutoNetworkedField]
    public int Range = 6;

    [DataField, AutoNetworkedField]
    public float Speed = 1;

    [DataField, AutoNetworkedField]
    public EntProtoId ProjectileId = "MCXenoProjectileFireball";

    [DataField, AutoNetworkedField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_MC/Effects/Pyrogen/fireball.ogg"); // sound/bullets/spear_armor1.ogg'
}
