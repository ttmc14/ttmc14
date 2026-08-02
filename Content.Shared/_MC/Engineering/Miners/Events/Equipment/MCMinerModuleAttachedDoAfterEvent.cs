using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Engineering.Miners.Events.Equipment;

[Serializable, NetSerializable]
public sealed partial class MCMinerModuleAttachedDoAfterEvent : SimpleDoAfterEvent;
