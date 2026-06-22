using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Armor.Modules.Core.Events;

[Serializable, NetSerializable]
public sealed partial class MCArmorModuleDeattachDoAfterEvent : SimpleDoAfterEvent;

