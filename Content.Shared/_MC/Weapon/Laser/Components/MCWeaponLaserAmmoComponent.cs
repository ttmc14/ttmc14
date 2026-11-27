using Content.Shared._MC.Weapon.Laser.Systems;
using Robust.Shared.GameStates;

namespace Content.Shared._MC.Weapon.Laser.Components;

/// <summary>
/// Marks a cell that a laser can use as its ammunition supply.
/// </summary>
[RegisterComponent, NetworkedComponent]
[Access(typeof(MCWeaponLaserSystem))]
public sealed partial class MCWeaponLaserAmmoComponent : Component;
