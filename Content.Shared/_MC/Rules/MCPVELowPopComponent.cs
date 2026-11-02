// Подключаем необходимые библиотеки из движка и игры
using Robust.Shared.Prototypes; // Для работы с прототипами (шаблонами сущностей, профессий)
using Robust.Shared.GameStates; // Для синхронизации данных между сервером и клиентом
using Robust.Shared.Serialization; // Для сохранения и загрузки состояний
using Content.Shared._RMC14.Weapons.Ranged.IFF; // Для системы распознавания "свой-чужой" (фракции)
using Content.Shared.Roles; // Для работы с профессиями (JobPrototype)

// Создаем новое, чистое пространство имен для нашего режима
namespace Content.Shared._MC.Rules.MCPVEDefendLowPopComponent;

/// <summary>
/// Компонент для PVE-режима "PVEDefendLowPop".
/// Этот компонент является контейнером данных: он хранит все уникальные
/// настройки для этого режима. Вся логика (спавн врагов, проверка победы)
/// будет находиться в парном ему файле-системе (PVEDefendLowPopRuleSystem.cs).
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
public sealed partial class PVEDefendLowPopRuleComponent : Component
{
    // --- СЕКЦИЯ МОРПЕХОВ (ИГРОКОВ) ---

    /// <summary>
    /// Прототип фракции, к которой принадлежат игроки.
    /// Стандартно - "FactionMarine". Это нужно, чтобы турели и снаряжение
    /// правильно опознавали игроков как союзников.
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId<IFFFactionComponent> MarineFaction = "FactionMarine";

    /// <summary>
    /// Список ID профессий, которые доступны игрокам в этом режиме.
    /// Сюда нужно будет вписать ID всех ролей морпехов из TTMC14,
    /// которые ты хочешь разрешить (например, "RMCSquadLeader", "RMCResearcher").
    /// Это позволяет гибко настраивать доступные классы.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<ProtoId<JobPrototype>> AllowedMarineJobs = new();

    /// <summary>
    /// Время в минутах, через которое погибший морпех может возродиться.
    /// Для динамичного PVE-режима стоит сделать его не очень большим.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan MarineRespawnTime = TimeSpan.FromMinutes(7);


    // --- СЕКЦИЯ КСЕНОМОРФОВ (ИИ) ---

    /// <summary>
    /// Прототип фракции для ИИ-противников. Обычно "FactionXeno".
    /// </summary>
    [DataField, AutoNetworkedField]
    public EntProtoId<IFFFactionComponent> EnemyFaction = "FactionXeno";

    /// <summary>
    /// Список прототипов сущностей (EntProtoId) ксеноморфов или других угроз,
    /// которые будут спавниться в качестве противников.
    /// Например: "MobXenoRunner", "MobXenoDroneAI".
    /// Система будет случайным образом выбирать одного из них для спавна.
    /// </summary>
    [DataField, AutoNetworkedField]
    public List<EntProtoId> XenoPrototypes = new();

    /// <summary>
    /// Как часто система будет пытаться заспавнить новую волну или одного врага.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan SpawnInterval = TimeSpan.FromSeconds(30);

    /// <summary>
    /// Максимальное количество ИИ-противников, которое может быть на карте одновременно.
    /// Это нужно для контроля производительности и сложности.
    /// </summary>
    [DataField, AutoNetworkedField]
    public int MaxAiCount = 20;


    // --- УСЛОВИЯ И СОСТОЯНИЕ РАУНДА ---

    /// <summary>
    /// Основная цель: сколько времени морпехам нужно продержаться для победы.
    /// </summary>
    [DataField, AutoNetworkedField]
    public TimeSpan DefendTime = TimeSpan.FromMinutes(35);

    /// <summary>
    /// Текущее состояние игрового режима. Помогает системе понимать,
    /// что делать в данный момент: подготавливаться, активно работать или завершать раунд.
    /// </summary>
    [DataField, AutoNetworkedField]
    public PVEDefendLowPopState RoundState = PVEDefendLowPopState.NotStarted;

    /// <summary>
    /// Хранит итоговый результат раунда.
    /// </summary>
    [DataField]
    public PVEDefendLowPopResult Result = PVEDefendLowPopResult.None;
}

/// <summary>
/// Возможные состояния игрового режима.
/// </summary>
[Serializable, NetSerializable]
public enum PVEDefendLowPopState
{
    NotStarted, // Режим еще не начался
    InProgress, // Режим активен, идет бой
    Finished    // Режим завершен, показан результат
}

/// <summary>
/// Возможные исходы раунда.
/// </summary>
[Serializable, NetSerializable]
public enum PVEDefendLowPopResult
{
    None,         // Результат еще не определен
    MarinesWin,   // Морпехи победили (продержались нужное время)
    MarinesLose   // Морпехи проиграли (все погибли)
}
