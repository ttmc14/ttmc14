using Content.Shared._MC.AI.Modules;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.AI.Sensors;

/// <summary>
/// Определяет настройки для сенсора, который ищет ближайшую точку защиты (MarineDefendPoint).
/// </summary>
[DataDefinition]
public sealed partial class MCAISensorDefendPoint : MCAISensor<MCAISensorDefendPoint>
{
    // Не забываем создать пустой конструктор для сериализации
    public MCAISensorDefendPoint() { }

    /// <summary>
    /// Ключ в памяти ИИ, куда будет записан результат (true/false) - найдена ли точка.
    /// </summary>
    [DataField("conditionKey")]
    public string ConditionKey { get; private set; } = "DefendPointExists";

    /// <summary>
    /// Ключ в памяти ИИ, куда будет сохранена сама найденная сущность (точка).
    /// </summary>
    [DataField("outputTargetKey")]
    public string OutputTargetKey { get; private set; } = "DefendPointTarget";
}
