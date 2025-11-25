using System.Linq;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;

namespace Content.Shared._MC.Chemistry;

public sealed class MCSolutionTickerSystem : EntitySystem
{
    private EntityQuery<MCSolutionTickerComponent> _solutionTickerQuery;

    public override void Initialize()
    {
        base.Initialize();

        _solutionTickerQuery = GetEntityQuery<MCSolutionTickerComponent>();
    }

    public int GetTick(EntityUid target, Solution solution, ReagentPrototype reagent)
    {
        if (!_solutionTickerQuery.TryComp(target, out var component))
            return 0;

        if (!component.Entries.TryGetValue(solution, out var entries))
            return 0;

        return (from entry in entries where entry.Reagent.Prototype == reagent.ID select entry.Ticks).FirstOrDefault();
    }
}
