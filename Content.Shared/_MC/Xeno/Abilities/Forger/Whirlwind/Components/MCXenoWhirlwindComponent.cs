using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Forger.Whirlwind.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(fieldDeltas: true)]
[Access(typeof(MCXenoWhirlwindSystem))]
public sealed partial class MCXenoWhirlwindComponent : Component
{
    #region Projectile

    /// <summary>
    /// Time the Forger must stand still before launching the whirlwind.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan ProjectileDelay = TimeSpan.FromSeconds(0.6);

    /// <summary>
    /// Whirlwind projectile spawned when the wind-up completes.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId<MCXenoWhirlwindProjectileComponent> ProjectileId = "MCXenoProjectileWhirlwind";

    #endregion

    /// <summary>
    /// Sound played when the whirlwind launches.
    /// </summary>
    [DataField, AutoNetworkedField]
    public SoundSpecifier Sound = new SoundPathSpecifier("/Audio/_MC/Effects/Pyrogen/prepare.ogg");
}
