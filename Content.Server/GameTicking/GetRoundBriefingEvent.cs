using Content.Shared._MC.RoundInfo;

namespace Content.Server.GameTicking;

[ByRefEvent]
public readonly record struct GetRoundBriefingEvent(EntityUid? Viewer, List<string> Lines, MCRoundInfoReserves? Reserves = null);

public sealed class RoundInfoUpdatedEvent : EntityEventArgs;
