using Content.Server._MC.Bomb.Systems;
using Content.Server._MC.Explosion.EntitySystems;
using Content.Server.Explosion.Components;
using Robust.Shared.Audio;

namespace Content.Server._MC.Bomb.Components;

/// <summary>
/// This is used for bombs that should be defused (MC variant).
/// The explosion configuration should be handled by <see cref="ExplosiveComponent"/>.
/// </summary>
[RegisterComponent, Access(typeof(MCDefusableSystem))]
public sealed partial class MCDefusableComponent : Component
{
    [ViewVariables(VVAccess.ReadOnly), DataField("defusalSound")]
    public SoundSpecifier DefusalSound = new SoundPathSpecifier("/Audio/Misc/notice2.ogg");

    [ViewVariables(VVAccess.ReadOnly), DataField("boltSound")]
    public SoundSpecifier BoltSound = new SoundPathSpecifier("/Audio/Machines/boltsdown.ogg");

    [ViewVariables(VVAccess.ReadWrite), DataField("disposable")]
    public bool Disposable = true;

    [ViewVariables(VVAccess.ReadWrite), DataField("activated")]
    public bool Activated;

    [ViewVariables(VVAccess.ReadWrite)]
    public bool Usable = true;

    [ViewVariables(VVAccess.ReadWrite)]
    public bool DisplayTime = true;

    [ViewVariables(VVAccess.ReadWrite)]
    public bool Bolted;

    [DataField("delayTime")]
    public int DelayTime = 30;

    #region Wires
    [ViewVariables(VVAccess.ReadWrite), Access(Other=AccessPermissions.ReadWrite)]
    public bool DelayWireUsed;

    [ViewVariables(VVAccess.ReadWrite), Access(Other=AccessPermissions.ReadWrite)]
    public bool ProceedWireCut;

    [ViewVariables(VVAccess.ReadWrite), Access(Other=AccessPermissions.ReadWrite)]
    public bool ProceedWireUsed;

    [ViewVariables(VVAccess.ReadWrite), Access(Other=AccessPermissions.ReadWrite)]
    public bool ActivatedWireUsed;

    #endregion
}
