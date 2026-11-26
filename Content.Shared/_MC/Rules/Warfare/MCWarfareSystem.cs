using Content.Shared.GameTicking.Components;

namespace Content.Shared._MC.Rules.Warfare;

public sealed class MCWarfareSystem : EntitySystem
{
    /// <summary>
    /// Gets the active server-side Warfare rule, including its remaining reserves.
    /// </summary>
    public MCRuleWarfareComponent? ActiveRule
    {
        get
        {
            var query = EntityQueryEnumerator<MCRuleWarfareComponent, ActiveGameRuleComponent>();
            return query.MoveNext(out _, out var rule, out _) ? rule : null;
        }
    }
}
