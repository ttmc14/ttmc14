using Robust.Shared.GameObjects;

namespace Content.Shared._MC.AI.Components;

/// <summary>
/// Это компонент-маркер. Прикрепите его к любой сущности на карте (например, к невидимому маркеру),
/// чтобы ИИ распознавал ее как точку, которую нужно защищать или к которой нужно двигаться.
/// </summary>
[RegisterComponent]
public sealed partial class MCMarineDefendPointComponent : Component
{
}
