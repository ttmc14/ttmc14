namespace Content.Shared._MC.Respawn;

[ByRefEvent]
public record struct MCGetRespawnDelayEvent(TimeSpan Delay);
