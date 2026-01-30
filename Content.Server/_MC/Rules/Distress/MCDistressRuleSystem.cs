using Content.Server._MC.Xeno;
using Content.Server._MC.Xeno.Hive;
using Content.Server._MC.Xeno.Spawn;
using Content.Server.GameTicking;
using Content.Server.RoundEnd;
using Content.Shared._MC;
using Content.Shared._MC.Nuke.Bomb.Events;
using Content.Shared._MC.Operation;
using Content.Shared._MC.Rules;
using Content.Shared._MC.Xeno.Hive.Events;
using Content.Shared._RMC14.Dropship;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Systems;
using Robust.Server.Audio;
using Robust.Shared.Configuration;

namespace Content.Server._MC.Rules.Distress;

public sealed partial class MCDistressRuleSystem : MCRuleSystem<MCDistressSignalRuleComponent>
{
    [Dependency] private readonly IConfigurationManager _config = null!;

    [Dependency] private readonly AudioSystem _audio = null!;
    [Dependency] private readonly RoundEndSystem _roundEnd = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;

    [Dependency] private readonly MCRuleStartValidationSystem _mcRuleStartValidation = null!;
    [Dependency] private readonly MCOperationStartSystem _mcOperationStart = null!;
    [Dependency] private readonly MCXenoHiveSystem _mcXenoHive = null!;
    [Dependency] private readonly MCXenoSpawnSystem _mcXenoSpawn = null!;
    [Dependency] private readonly MCXenoSpawnFlowSystem _mcXenoSpawnFlow = null!;

    private TimeSpan _forceEndHijackTime;

    public override void Initialize()
    {
        base.Initialize();

        Subs.CVar(_config, MCConfigVars.RoundForceEndHijackTimeMinutes, v => _forceEndHijackTime = TimeSpan.FromMinutes(v), true);

        SubscribeLocalEvent<LoadingMapsEvent>(OnMapLoading);
        SubscribeLocalEvent<RulePlayerSpawningEvent>(OnRulePlayerSpawning);

        SubscribeLocalEvent<MCNukeExplodedEvent>(OnNukeExploded);
        SubscribeLocalEvent<MCXenoHiveCollapsed>(OnHiveCollapsed);

        SubscribeLocalEvent<DropshipHijackStartEvent>(OnDropshipHijackStart);
        SubscribeLocalEvent<DropshipHijackLandedEvent>(OnDropshipHijackLanded);
    }

    protected override void OnStartAttempt(
        Entity<MCDistressSignalRuleComponent, GameRuleComponent> gameRule,
        RoundStartAttemptEvent ev)
    {
        if (ev.Forced || ev.Cancelled)
            return;

        var rule = gameRule.Comp1;
        if (_mcRuleStartValidation.TryValidateXenoRequirements(
                ev.Players,
                rule.XenoSelectableJob,
                rule.ShrikeJob,
                minPlayers: 2,
                minXenoCandidates: 1,
                out var xenoCandidates,
                out var failReason))
            return;

        _mcRuleStartValidation.AnnounceFail(failReason);
        ev.Cancel();
    }

    private void OnMapLoading(LoadingMapsEvent ev)
    {
        if (!GameTicker.IsGameRuleAdded<MCDistressSignalRuleComponent>())
            return;

        _mcXenoSpawn.SelectRandomPlanet();

        GameTicker.UpdateInfoText();
    }

    private void OnRulePlayerSpawning(RulePlayerSpawningEvent ev)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var component, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            if (!_mcXenoSpawn.SpawnXenoMap<MCDistressSignalRuleComponent>((uid, component)))
                continue;

            // Apply default hive settings
            SetupHive(component);

#if !FULL_RELEASE
            var duartion = TimeSpan.FromSeconds(30);
#else
            var duartion = TimeSpan.FromMinutes(10);
#endif

            _mcOperationStart.StartWithDelay(duartion);

            StartBioscan();
            SpawnAdminAreas(component.Thunderdome);

            RefreshIFF(component.MarineFaction);
            RefreshFaxes();

            _mcXenoSpawnFlow.Spawn(
                ev,
                component.ShrikeJob,
                component.ShrikeEnt,
                component.XenoSelectableJob,
                component.LarvaEnt,
                GetXenos(ev.PlayerPool.Count));
        }
    }
}
