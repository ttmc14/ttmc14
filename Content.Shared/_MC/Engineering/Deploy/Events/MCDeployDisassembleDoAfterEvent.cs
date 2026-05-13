using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Engineering.Deploy.Events;

[Serializable, NetSerializable]
public sealed partial class MCDeployDisassembleDoAfterEvent : SimpleDoAfterEvent;
