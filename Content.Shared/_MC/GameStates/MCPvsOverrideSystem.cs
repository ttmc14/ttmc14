using Robust.Shared.Player;

namespace Content.Shared._MC.GameStates;

public abstract class MCPvsOverrideSystem : EntitySystem
{
    /// <summary>
    /// This causes an entity and all of its parents to always be sent to a player.
    /// </summary>
    /// <remarks>
    /// This differs from AddSessionOverride as it does not send children, will ignore a players usual
    /// PVS budget, and ignores visibility masks. You generally shouldn't use this unless an entity absolutely always
    /// needs to be sent to a client.
    /// </remarks>
    public abstract void AddForceSend(EntityUid uid, ICommonSession session);

    /// <summary>
    /// Removes an entity from a session's force send set.
    /// </summary>
    public abstract void RemoveForceSend(EntityUid uid, ICommonSession session);
}
