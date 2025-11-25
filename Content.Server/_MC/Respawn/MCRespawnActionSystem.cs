using Content.Server.GameTicking;
using Content.Shared.Actions;
using Content.Server.Popups;
using Content.Shared._MC.Respawn;
using Content.Shared.Popups;
using Content.Shared.Mind;
using Content.Shared._MC;
using Robust.Server.GameObjects;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using Robust.Shared.Configuration;

namespace Content.Server._MC.Respawn;

public sealed partial class MCRespawnActionSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _cfg = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly PopupSystem _popup = default!;
    [Dependency] private readonly SharedMindSystem _mind = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MCRespawnActionComponent, MCRespawnActionEvent>(OnRespawnAction);
            SubscribeLocalEvent<MCRespawnActionComponent, MapInitEvent>(OnRespawnMapInit);
    }

    private void OnRespawnAction(Entity<MCRespawnActionComponent> ent, ref MCRespawnActionEvent args)
    {
        var player = ent.Owner;
        if (!TryComp<ActorComponent>(player, out var actor) || actor.PlayerSession == null)
            return;

        var userId = actor.PlayerSession.UserId;

            TimeSpan? deathTime = null;
            if (TryComp<Content.Shared.Ghost.GhostComponent>(player, out var ghost))
                deathTime = ghost.TimeOfDeath;
            else if (_mind.TryGetMind(userId, out var mindId, out var mind) && mind != null)
                deathTime = mind.TimeOfDeath;

            if (deathTime == null || deathTime == TimeSpan.Zero)
                return;

            var timeSinceDeath = _timing.CurTime - deathTime.Value;
            var minutes = _cfg.GetCVar(MCConfigVars.MCRespawnMarinesActionCooldownMinutes);
            var required = TimeSpan.FromMinutes(minutes);
            if (timeSinceDeath < required)
            {
                var left = required - timeSinceDeath;
                var msg = Loc.GetString("respawn-action-popup", ("minutes", left.Minutes.ToString("D2")), ("seconds", left.Seconds.ToString("D2")));
                _popup.PopupEntity(msg, player, player, PopupType.LargeCaution);
                return;
            }

            _gameTicker.Respawn(actor.PlayerSession);
    }

        private void OnRespawnMapInit(Entity<MCRespawnActionComponent> ent, ref MapInitEvent args)
        {
            var actions = EntitySystem.Get<SharedActionsSystem>();
            actions.AddAction(ent, ref ent.Comp.Action, ent.Comp.ActionId);
        }
}
