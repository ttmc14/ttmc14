namespace Content.Shared._MC.Xeno.Abilities.Pounce;

[ByRefEvent]
public readonly record struct MCXenoPounceHitEvent(EntityUid TargetUid, bool First);
