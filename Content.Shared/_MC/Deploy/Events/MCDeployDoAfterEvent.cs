using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Deploy.Events;

[Serializable, NetSerializable]
public sealed partial class MCDeployDoAfterEvent : SimpleDoAfterEvent;
