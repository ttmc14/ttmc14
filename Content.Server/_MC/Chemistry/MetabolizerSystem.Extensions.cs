using System.Diagnostics.CodeAnalysis;
using Content.Shared._MC.Chemistry;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Reagent;

// ReSharper disable once CheckNamespace
namespace Content.Server.Body.Systems;

public partial class MetabolizerSystem
{
    // ReSharper disable once UseCollectionExpression
    private readonly ReagentId[] _canTick = new ReagentId[]
    {
        new ("MCNeurotoxin", null),
        new ("MCNanoMachines", null),
    };

    // ReSharper disable once UseCollectionExpression
    private readonly List<EntityUid> _updated = new();

    private void UpdateExtension(float _)
    {
        _updated.Clear();
    }

    private void ClearTickMetabolize(EntityUid uid, Solution solution)
    {
        if (!TryTick(uid, solution, out var tickerComponent))
            return;

        if (!tickerComponent.Entries.TryGetValue(solution, out var entries))
            return;

        foreach (var entry in entries)
        {
            entry.Ticks = 0;
        }
    }

    private void BeforeMetabolize(EntityUid uid, Solution solution)
    {
        if (!TryTick(uid, solution, out var tickerComponent))
            return;

        if (!tickerComponent.Entries.TryGetValue(solution, out var entries))
        {
            entries = new List<MCSolutionTickerComponent.TickEntry>();
            foreach (var reagentId in _canTick)
            {
                entries.Add(new MCSolutionTickerComponent.TickEntry(reagentId, -1));
            }

            tickerComponent.Entries[solution] = entries;
        }

        foreach (var entry in entries)
        {
            if (!solution.TryGetReagent(entry.Reagent, out _))
            {
                entry.Ticks = 0;
                continue;
            }

            entry.Ticks++;
        }
    }

    private bool TryTick(EntityUid uid, Solution solution, [NotNullWhen(true)] out MCSolutionTickerComponent? tickerComponent)
    {
        tickerComponent = null;

        if (solution.Name != "chemicals")
            return false;

        if (!TryComp(uid, out tickerComponent))
            return false;

        if (_updated.Contains(uid))
            return false;

        _updated.Add(uid);
        return true;
    }
}
