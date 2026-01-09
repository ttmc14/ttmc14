using Content.Shared.Actions;

namespace Content.Shared._MC.Zombies;

/// <summary>
///     Event that is broadcast whenever an entity is zombified.
///     Used by the zombie gamemode to track total infections.
/// </summary>
[ByRefEvent]
public readonly struct MCEntityZombifiedEvent
{
    /// <summary>
    ///     The entity that was zombified.
    /// </summary>
    public readonly EntityUid Target;

    public MCEntityZombifiedEvent(EntityUid target)
    {
        Target = target;
    }
};

/// <summary>
///     Event raised when a player zombifies themself using the "turn" action
/// </summary>
public sealed partial class MCZombifySelfActionEvent : InstantActionEvent { };
