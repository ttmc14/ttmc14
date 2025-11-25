namespace Content.Shared._MC.FTL.Events;

[ByRefEvent]
public readonly record struct MCFTLEndEvent(EntityUid Entity, EntityUid MapUid);
