using Robust.Server.GameStates;
using Robust.Shared.Player;

namespace Content.Server._MC.GameStates;

public sealed class MCPvsOverrideSystem : Content.Shared._MC.GameStates.MCPvsOverrideSystem
{
    [Dependency] private readonly PvsOverrideSystem _pvsOverride = null!;

    /// <inheritdoc/>
    public override void AddForceSend(EntityUid uid, ICommonSession session)
    {
        _pvsOverride.AddForceSend(uid, session);
    }

    /// <inheritdoc/>
    public override void RemoveForceSend(EntityUid uid, ICommonSession session)
    {
        _pvsOverride.RemoveForceSend(uid, session);
    }
}
