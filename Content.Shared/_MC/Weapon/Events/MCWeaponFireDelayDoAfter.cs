using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Weapon.Events;

[Serializable, NetSerializable]
public sealed partial class MCWeaponFireDelayDoAfter : SimpleDoAfterEvent;
