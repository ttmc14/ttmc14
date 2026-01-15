namespace Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSelector.Events;

/// <summary>
///  Need for providing selection to others systems, without create news
/// </summary>
/// <param name="Key"></param>
[ByRefEvent]
public readonly record  struct MCXenoReagentSelectorSetEvent(string Key);
