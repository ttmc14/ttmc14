﻿using Content.Server._MC.Xeno;
using Content.Server._MC.Xeno.Hive;
using Content.Server._MC.Xeno.Spawn;
using Content.Server._RMC14.Power;
using Content.Server.GameTicking;
using Content.Server.Maps;
using Content.Server.Preferences.Managers;
using Content.Server.RoundEnd;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared._MC.Living;
using Content.Shared._MC.Nuke.Bomb.Events;
using Content.Shared._MC.Rules.Crash;
using Content.Shared._MC.Shuttle.Events;
using Content.Shared._MC.Xeno.Hive.Events;
using Content.Shared._RMC14.Marines;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Server._MC.Rules.Crash;

public sealed partial class MCCrashRuleSystem : MCRuleSystem<MCCrashRuleComponent>
{
    [Dependency] private readonly IPrototypeManager _prototype = null!;
    [Dependency] private readonly IRobustRandom _random = null!;

    [Dependency] private readonly RMCPowerSystem _rmcPower = null!;
    [Dependency] private readonly ShuttleSystem _shuttle = null!;
    [Dependency] private readonly RoundEndSystem _roundEnd = null!;

    [Dependency] private readonly MCRuleStartValidationSystem _mcRuleStartValidation = null!;
    [Dependency] private readonly MCXenoAutoSpawnBalanceSystem _mcXenoAutoSpawnBalance = null!;
    [Dependency] private readonly MCXenoHiveSystem _mcXenoHive = null!;
    [Dependency] private readonly MCXenoSpawnSystem _mcXenoSpawn = null!;
    [Dependency] private readonly MCXenoSpawnFlowSystem _mcXenoSpawnFlow = null!;
    [Dependency] private readonly MCLivingSystem _mcLiving = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<LoadingMapsEvent>(OnMapLoading);
        SubscribeLocalEvent<RulePlayerSpawningEvent>(OnRulePlayerSpawning);

        SubscribeLocalEvent<MCShuttleEvacuationEvent>(OnShuttleEvacuationEvent);
        SubscribeLocalEvent<MCNukeExplodedEvent>(OnNukeExploded);
        SubscribeLocalEvent<MCXenoHiveCollapsed>(OnHiveCollapsed);

        SubscribeLocalEvent<MarineComponent, MobStateChangedEvent>(OnMobStateChanged);
        SubscribeLocalEvent<MarineComponent, ComponentRemove>(OnCompRemove);
    }

    protected override void OnStartAttempt(
        Entity<MCCrashRuleComponent, GameRuleComponent> gameRule,
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
                out _,
                out var failReason))
            return;

        _mcRuleStartValidation.AnnounceFail(failReason);
        ev.Cancel();
    }

    protected override void ActiveTick(EntityUid uid, MCCrashRuleComponent component, GameRuleComponent gameRule, float frameTime)
    {
        base.ActiveTick(uid, component, gameRule, frameTime);
        if (component is not IRuleRecalculatePower recalculatePower)
            return;

        if (recalculatePower.PowerRecalculated)
            return;

        _rmcPower.RecalculatePower();

        recalculatePower.PowerRecalculated = true;
        Dirty(uid, component);
    }


    private void OnMapLoading(LoadingMapsEvent ev)
    {
        if (!GameTicker.IsGameRuleAdded<MCCrashRuleComponent>())
            return;

        ev.Maps.Clear();
        ev.Maps.Add(_prototype.Index<GameMapPrototype>("MCCanterbury"));

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

            RulePlayerSpawning(uid, component, ev);
        }
    }

    private void RulePlayerSpawning(EntityUid uid, MCCrashRuleComponent component, RulePlayerSpawningEvent ev)
    {
        if (!_mcXenoSpawn.SpawnXenoMap<MCCrashRuleComponent>((uid, component)))
            return;

        // Apply default hive settings
        SetupHive(component);

        // Enable xeno auto spawn
        _mcXenoAutoSpawnBalance.Enable();

        SpawnAdminAreas(component.Thunderdome);
        SpawnNukeDiskGenerators();

        RefreshIFF(component.MarineFaction);
        RefreshFaxes();

        CrashShuttle(component.ShuttleCrushTime);

        _mcXenoSpawnFlow.Spawn(
            ev,
            component.ShrikeJob,
            component.ShrikeEnt,
            component.XenoSelectableJob,
            component.LarvaEnt,
            GetXenos(ev.PlayerPool.Count));
    }

    private void CrashShuttle(TimeSpan flyTime)
    {
        var crashPoints= _mcXenoSpawnFlow.GetEntities<MCCrashPointComponent>();
        if (crashPoints.Count == 0)
        {
            Log.Fatal("Fuck!");
            return;
        }

        var target = _random.Pick(crashPoints);
        var offset = Comp<MCCrashPointComponent>(target).Offset;
        var coords = Transform(target).Coordinates.Offset(offset);

        var query = EntityQueryEnumerator<AlmayerComponent, ShuttleComponent>();
        while (query.MoveNext(out var uid, out _, out var shuttle))
        {
            _shuttle.FTLToCoordinates(
                uid,
                shuttle,
                coords,
                Angle.Zero,
                hyperspaceTime: (float) flyTime.TotalSeconds);
        }
    }
}
