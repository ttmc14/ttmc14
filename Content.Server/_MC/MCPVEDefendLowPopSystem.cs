using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.Spawners.Components;
using Content.Server.Mind;
using Content.Shared.NPC.Components;
using Content.Shared.Mind.Components;
using Content.Shared._MC.Rules;
using Content.Shared._MC.Rules.PVEDefendLowPopRuleComponent;
using Robust.Shared.Random;
using Robust.Shared.Timing;
using System.Linq;
using Content.Server.Fluids.EntitySystems;
using Content.Shared.Chemistry.Components;

namespace Content.Server._MC.Rules.PVEDefendLowPopRule;

public sealed class MCPVEDefendLowPopRuleSystem : GameRuleSystem<MCPVEDefendLowPopRuleComponent>
{
    // --- ИНЪЕКЦИЯ ЗАВИСИМОСТЕЙ ---
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;
    [Dependency] private readonly SmokeSystem _smoke = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    private TimeSpan _roundTime = TimeSpan.Zero;
    private TimeSpan _spawnCheckTime = TimeSpan.Zero;

    // --- ОСНОВНЫЕ МЕТОДЫ СИСТЕМЫ ---

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RoundStartAttemptEvent>(OnRoundStart);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!TryGetActiveRule(out var uid, out var rule))
            return;

        if (rule.RoundState != MCPVEDefendLowPopRuleState.InProgress)
            return;

        var frameTimeSpan = TimeSpan.FromSeconds(frameTime);
        _roundTime += frameTimeSpan;
        _spawnCheckTime += frameTimeSpan;

        // --- ПРОВЕРКА УСЛОВИЙ ПОБЕДЫ И ПОРАЖЕНИЯ ---

        if (_roundTime >= rule.DefendTime)
        {
            DoVictory(uid, rule);
            return;
        }

        var query = EntityQuery<MindContainerComponent>(true);
        var marinesAlive = query.Any(mc => mc.HasMind);

        if (!marinesAlive)
        {
            DoDefeat(uid, rule);
            return;
        }

        // --- ЛОГИКА СПАВНА ВРАГОВ ---

        if (_spawnCheckTime >= rule.SpawnInterval)
        {
            _spawnCheckTime = TimeSpan.Zero;
            TrySpawnEnemy(rule);
        }
    }

    // --- МЕТОДЫ-ПОМОЩНИКИ ---

    private bool TryGetActiveRule(out EntityUid uid, out MCPVEDefendLowPopRuleComponent rule)
    {
        foreach (var candidate in EntityQuery<MCPVEDefendLowPopRuleComponent>(true))
        {
            uid = candidate.Owner;
            rule = candidate;
            return true;
        }

        uid = default;
        rule = default!;
        return false;
    }

    private void OnRoundStart(RoundStartAttemptEvent ev)
    {
        if (!TryGetActiveRule(out _, out var rule))
            return;

        rule.RoundState = MCPVEDefendLowPopRuleState.InProgress;
        _roundTime = TimeSpan.Zero;
        _spawnCheckTime = TimeSpan.Zero;

        Logger.Info("PVEDefendLowPop game mode has started!");
    }

    private void TrySpawnEnemy(MCPVEDefendLowPopRuleComponent rule)
    {
        if (rule.XenoPrototypes.Count == 0)
            return;

        var enemyQuery = EntityQuery<NpcFactionMemberComponent>();
        var factionId = rule.EnemyFaction.Id;
        var currentEnemyCount = enemyQuery.Count(c => c.Factions.Contains(factionId));

        if (currentEnemyCount >= rule.MaxAiCount)
            return;

        var spawnPoints = EntityQuery<SpawnPointComponent>(true).ToList();

        if (spawnPoints.Count == 0)
        {
            Logger.Warning("MCPVEDefendLowPop: No spawn points found on the map!");
            return;
        }

        var spawnPoint = _random.Pick(spawnPoints);
        var enemyPrototype = _random.Pick(rule.XenoPrototypes);
        var coords = Transform(spawnPoint.Owner).Coordinates;

        var spawned = Spawn(enemyPrototype, coords);

        EnsureComp<NpcFactionMemberComponent>(spawned);
        var factionComp = Comp<NpcFactionMemberComponent>(spawned);
        factionComp.Factions.Add(factionId);

        if (rule.SpawnSmokeOnSpawn)
        {
            var solution = new Solution();
            solution.AddReagent(rule.SmokeReagent, rule.SmokeReagentAmount);

            var smokeEnt = Spawn(rule.SmokePrototype, coords);
            _smoke.StartSmoke(smokeEnt, solution, rule.SmokeDuration, rule.SmokeSpread);
        }

        Logger.Info($"Spawned '{enemyPrototype}' at {_transform.GetMapCoordinates(spawned).Position}" +
                    (rule.SpawnSmokeOnSpawn ? " with smoke." : "."));
    }

    private void DoVictory(EntityUid uid, MCPVEDefendLowPopRuleComponent rule)
    {
        if (rule.Result != MCPVEDefendLowPopRuleResult.None)
            return;

        rule.Result = MCPVEDefendLowPopRuleResult.MarinesWin;
        rule.RoundState = MCPVEDefendLowPopRuleState.Finished;

        _gameTicker.EndRound("Морпехи успешно продержались и отразили атаку!");
        Logger.Info("PVEDefendLowPop: Marines win!");
    }

    private void DoDefeat(EntityUid uid, MCPVEDefendLowPopRuleComponent rule)
    {
        if (rule.Result != MCPVEDefendLowPopRuleResult.None)
            return;

        rule.Result = MCPVEDefendLowPopRuleResult.MarinesLose;
        rule.RoundState = MCPVEDefendLowPopRuleState.Finished;

        _gameTicker.EndRound("Все морпехи пали. Миссия провалена.");
        Logger.Info("PVEDefendLowPop: Marines lose!");
    }
}
