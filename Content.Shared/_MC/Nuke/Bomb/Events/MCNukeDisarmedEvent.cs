namespace Content.Shared._MC.Nuke.Bomb.Events;

public sealed class MCNukeDisarmedEvent(bool disarmedByXeno = false) : EntityEventArgs
{
    public bool DisarmedByXeno { get; } = disarmedByXeno;
}
