using Robust.Shared.Player;

namespace Content.Shared._MC.Xeno.Hive.Join;

[ByRefEvent]
public record struct MCBeforeJoinBurrowedLarvaEvent(ICommonSession Player)
{
    public bool Cancelled;
}
