using Content.Server.GameTicking;
using Content.Shared.Actions;
using Content.Server.Popups;
using Content.Shared._MC.Respawn;
using Content.Shared.Popups;
using Content.Shared.Mind;
using Content.Shared._MC._Config;
using Content.Shared.Ghost;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Configuration;

namespace Content.Server._MC.Respawn;

public sealed class MCRespawnActionSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configuration = null!;
    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly GameTicker _gameTicker = null!;
    [Dependency] private readonly PopupSystem _popup = null!;
    [Dependency] private readonly SharedMindSystem _mind = null!;
    [Dependency] private readonly SharedActionsSystem _actions = null!;

    private TimeSpan _respawnCooldown;

    public override void Initialize()
    {
        _configuration.OnValueChanged(MCConfigVars.RespawnMarinesActionCooldownMinutes, value => _respawnCooldown = TimeSpan.FromMinutes(value), true);

        SubscribeLocalEvent<MCRespawnActionComponent, MapInitEvent>(OnRespawnMapInit);
        SubscribeLocalEvent<MCRespawnActionComponent, MCRespawnActionEvent>(OnRespawnAction);
    }

    private void OnRespawnMapInit(Entity<MCRespawnActionComponent> entity, ref MapInitEvent args)
    {
        _actions.AddAction(entity, ref entity.Comp.Action, entity.Comp.ActionId);
    }

    private void OnRespawnAction(Entity<MCRespawnActionComponent> entity, ref MCRespawnActionEvent args)
    {
        if (!TryComp<ActorComponent>(entity, out var actor))
            return;

        var userId = actor.PlayerSession.UserId;
        var deathTime = TryComp<GhostComponent>(entity, out var ghost)
            ? ghost.TimeOfDeath
            : _mind.TryGetMind(userId, out _, out var mind)
                ? mind.TimeOfDeath
                : null;

        if (deathTime is null || deathTime == TimeSpan.Zero)
            return;

        var deathDelta = _timing.CurTime - deathTime.Value;
        var cooldownDelta = _respawnCooldown - deathDelta;

        if (cooldownDelta > TimeSpan.Zero)
        {
            var msg = Loc.GetString("respawn-action-popup", ("minutes", cooldownDelta.Minutes.ToString("D2")), ("seconds", cooldownDelta.Seconds.ToString("D2")));
            _popup.PopupEntity(msg, entity, entity, PopupType.LargeCaution);
            return;
        }

        _gameTicker.Respawn(actor.PlayerSession);
    }
}
