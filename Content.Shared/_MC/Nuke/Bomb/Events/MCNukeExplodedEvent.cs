namespace Content.Shared._MC.Nuke.Bomb.Events;

public sealed class MCNukeExplodedEvent(EntityUid bomb) : EntityEventArgs
{
    public readonly EntityUid Bomb = bomb;
}
