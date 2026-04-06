using Content.Shared.Physics;
using Content.Shared.Weapons.Ranged;
using Robust.Shared.Audio;
using Robust.Shared.GameStates;
using Robust.Shared.Prototypes;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Weapon.Laser.Components;

[RegisterComponent, NetworkedComponent, AutoGenerateComponentState(raiseAfterAutoHandleState: true, fieldDeltas: true)]
public sealed partial class MCWeaponLaserComponent : Component, IShootable
{
    [DataField, AutoNetworkedField]
    public string ContainerId = "gun_magazine";

    [DataField, AutoNetworkedField]
    public MCWeaponLaserFireMode? Mode;

    [DataField]
    public string StartingMode = "Standard";

    [DataField]
    public Dictionary<string, MCWeaponLaserFireMode> Modes = new();

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public int Shots;

    [ViewVariables(VVAccess.ReadWrite), AutoNetworkedField]
    public int Capacity;
}

[DataDefinition, Serializable]
public partial struct MCWeaponLaserFireMode()
{
    [DataField]
    public int CollisionMask = (int) CollisionGroup.Opaque;

    [DataField]
    public float MaxLength = 20f;

    [DataField]
    public EntProtoId EffectId = "MCEffectLaser";

    [DataField]
    public EntProtoId ProjectileId;

    [DataField]
    public int Shots = 50;

    [DataField]
    public float FireRate = 1f;

    [DataField]
    public SoundSpecifier? FireSound;

    [DataField]
    public SpriteSpecifier.Rsi? Icon;
}
