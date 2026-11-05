using Content.Server.GameTicking; // Нужен для доступа к событиям начала раунда
using Content.Server.GameTicking.Rules; // Базовый класс для игровых правил
using Content.Server.Spawners.Components; // Для работы с точками спавна
using Content.Server.Mind; // Для проверки, является ли сущность игроком
using Content.Shared.NPC.Components; // Для проверки вражеской фракции NPC
using Content.Shared.Mind.Components; // Для проверки, есть ли у сущности разум и является ли она игроком
using Content.Shared._MC.Rules.MCPVEDefendLowPopComponent; // Подключаем наш компонент
using Robust.Shared.Random; // Для выбора случайных врагов и точек спавна
using Robust.Shared.Timing; // Для отслеживания времени
using System.Linq; // Для удобной работы со списками

namespace Content.Server._MC.Rules.MCPVEDefendLowPopRule;

/// <summary>
/// Это СИСТЕМА, которая управляет логикой игрового режима PVEDefendLowPop.
/// Она использует данные из компонента PVEDefendLowPopRuleComponent для своей работы.
/// </summary>
public sealed class MCPVEDefendLowPopRuleSystem : GameRuleSystem<PVEDefendLowPopRuleComponent>
{
    // --- ИНЪЕКЦИЯ ЗАВИСИМОСТЕЙ ---
    // Движок автоматически подставит сюда нужные системы, чтобы мы могли ими пользоваться.
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly GameTicker _gameTicker = default!;

    // Внутренние переменные для отслеживания времени
    private TimeSpan _roundTime = TimeSpan.Zero;
    private TimeSpan _spawnCheckTime = TimeSpan.Zero;

    // --- ОСНОВНЫЕ МЕТОДЫ СИСТЕМЫ ---

    public override void Initialize()
    {
        base.Initialize();

        // Initialize the round timers when this game rule starts.
        SubscribeLocalEvent<RoundStartAttemptEvent>(OnRoundStart);
    }

    /// <summary>
    /// Этот метод вызывается на каждом "тике" сервера. Здесь происходит вся основная логика.
    /// </summary>
    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        // Ищем активное правило нашего режима. Если его нет, ничего не делаем.
        if (!TryGetActiveRule(out var uid, out var rule))
            return;

        // Если раунд еще не начался или уже закончился - выходим.
        if (rule.RoundState != PVEDefendLowPopState.InProgress)
            return;

        // Обновляем таймеры
        var frameTimeSpan = TimeSpan.FromSeconds(frameTime);
        _roundTime += frameTimeSpan;
        _spawnCheckTime += frameTimeSpan;

        // --- ПРОВЕРКА УСЛОВИЙ ПОБЕДЫ И ПОРАЖЕНИЯ ---

        // 1. Условие победы: вышло ли время?
        if (_roundTime >= rule.DefendTime)
        {
            DoVictory(uid, rule);
            return; // Завершаем выполнение, так как раунд окончен
        }

        // 2. Условие поражения: все ли морпехи мертвы?
        // Мы ищем всех живых игроков с ролью морпеха.
        var query = EntityQuery<MindContainerComponent>(true);
        var marinesAlive = query.Any(mc => mc.HasMind);

        if (!marinesAlive)
        {
            DoDefeat(uid, rule);
            return; // Завершаем, раунд окончен
        }

        // --- ЛОГИКА СПАВНА ВРАГОВ ---

        // Проверяем, пора ли спавнить новую волну
        if (_spawnCheckTime >= rule.SpawnInterval)
        {
            _spawnCheckTime = TimeSpan.Zero; // Сбрасываем таймер спавна
            TrySpawnEnemy(rule);
        }
    }

    // --- МЕТОДЫ-ПОМОЩНИКИ ---

    private bool TryGetActiveRule(out EntityUid uid, out PVEDefendLowPopRuleComponent rule)
    {
        foreach (var candidate in EntityQuery<PVEDefendLowPopRuleComponent>(true))
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
        // Ищем компонент правила и устанавливаем начальное состояние
        if (!TryGetActiveRule(out _, out var rule))
            return;

        rule.RoundState = PVEDefendLowPopState.InProgress;
        _roundTime = TimeSpan.Zero;
        _spawnCheckTime = TimeSpan.Zero;

        Logger.Info("PVEDefendLowPop game mode has started!");
    }


    private void TrySpawnEnemy(PVEDefendLowPopRuleComponent rule)
    {
        // Если прототипы врагов не заданы, выходим.
        if (rule.XenoPrototypes.Count == 0)
            return;

        // Считаем, сколько уже есть врагов на карте.
        // Для этого мы ищем всех NPC с вражеской фракцией.
        var enemyQuery = EntityQuery<NpcFactionMemberComponent>();
        var currentEnemyCount = enemyQuery.Count(c => c.Factions.Contains(rule.EnemyFaction.Id));

        if (currentEnemyCount >= rule.MaxAiCount)
            return; // Достигнут лимит, не спавним.

        // Находим все возможные точки спавна врагов (они должны быть на карте)
        var spawnPoints = EntityQuery<SpawnPointComponent>(true)
            .Where(s => s.SpawnType == SpawnPointType.Enemy)
            .ToList();

        if (spawnPoints.Count == 0)
        {
            Logger.Warning("PVEDefendLowPop: No enemy spawn points found on the map!");
            return;
        }

        // Выбираем случайную точку спавна и случайный прототип врага
        var spawnPoint = _random.Pick(spawnPoints);
        var enemyPrototype = _random.Pick(rule.XenoPrototypes);

        // Спавним!
        Spawn(enemyPrototype, Transform(spawnPoint.Owner).Coordinates);
        Logger.Info($"Spawned '{enemyPrototype}' at {Transform(spawnPoint.Owner).MapPosition}");
    }

    private void DoVictory(EntityUid uid, PVEDefendLowPopRuleComponent rule)
    {
        if (rule.Result != PVEDefendLowPopResult.None)
            return; // Предотвращаем двойное завершение раунда

        rule.Result = PVEDefendLowPopResult.MarinesWin;
        rule.RoundState = PVEDefendLowPopState.Finished;

        // Завершаем раунд и показываем сообщение всем игрокам
        _gameTicker.EndRound("Морпехи успешно продержались и отразили атаку!");
        Logger.Info("PVEDefendLowPop: Marines win!");
    }

    private void DoDefeat(EntityUid uid, PVEDefendLowPopRuleComponent rule)
    {
        if (rule.Result != PVEDefendLowPopResult.None)
            return;

        rule.Result = PVEDefendLowPopResult.MarinesLose;
        rule.RoundState = PVEDefendLowPopState.Finished;

        _gameTicker.EndRound("Все морпехи пали. Миссия провалена.");
        Logger.Info("PVEDefendLowPop: Marines lose!");
    }
}
