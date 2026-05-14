using Content.Server.EUI;
using Content.Server.Ghost;
using Content.Server.Mind;
using Content.Shared._MC.Medical.Revive;
using Robust.Server.Player;

namespace Content.Server._MC.Medical.Revive;

public sealed class MCReviveSystem : MCReviveSharedSystem
{
    [Dependency] private readonly IPlayerManager _player = null!;
    [Dependency] private readonly EuiManager _eui = null!;

    [Dependency] private readonly MindSystem _mind = null!;

    public override bool SendReviveRequest(EntityUid targetUid)
    {
        if (!_mind.TryGetMind(targetUid, out _, out var mind))
            return false;

        if (!_player.TryGetSessionById(mind.UserId, out var sessionFromMind))
            return false;

        if (mind.CurrentEntity == targetUid)
            return false;

        _eui.OpenEui(new ReturnToBodyEui(mind, _mind, _player), sessionFromMind);
        return true;
    }
}
