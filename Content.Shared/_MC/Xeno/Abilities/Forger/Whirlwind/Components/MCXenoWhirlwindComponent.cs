using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Forger.Whirlwind.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
[Access(typeof(MCXenoWhirlwindSystem))]
public sealed partial class MCXenoWhirlwindComponent : Component
{
    #region Projectile

    [DataField, AutoNetworkedField]
    public TimeSpan ProjectileDelay = TimeSpan.FromSeconds(0.6);

    [DataField, AutoNetworkedField]
    public float ProjectileSpeed = 5;

    [DataField, AutoNetworkedField]
    public EntProtoId ProjectileId = "MCXenoProjectileWhirlwind";

    #endregion

    [DataField, AutoNetworkedField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_MC/Effects/Pyrogen/prepare.ogg");
}
