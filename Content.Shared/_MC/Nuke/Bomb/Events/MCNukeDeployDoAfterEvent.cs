using Content.Shared.DoAfter;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Nuke.Bomb.Events;

[Serializable, NetSerializable]
public sealed partial class MCNukeDeployDoAfterEvent : SimpleDoAfterEvent;
