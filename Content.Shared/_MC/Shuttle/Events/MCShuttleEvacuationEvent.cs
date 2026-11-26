namespace Content.Shared._MC.Shuttle.Events;

public sealed class MCShuttleEvacuationEvent(bool completed = false) : EntityEventArgs
{
    public readonly bool Completed = completed;
}
