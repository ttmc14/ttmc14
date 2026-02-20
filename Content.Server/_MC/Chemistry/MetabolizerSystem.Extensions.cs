using System.Diagnostics.CodeAnalysis;
using Content.Server.Body.Components;
using Content.Shared._MC.Chemistry;
using Content.Shared._MC.Chemistry.Effects;
using Content.Shared._RMC14.Chemistry.Reagent;
using Content.Shared.Body.Organ;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.Components.SolutionManager;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;

// ReSharper disable once CheckNamespace
namespace Content.Server.Body.Systems;

public partial class MetabolizerSystem
{
    [Dependency] private readonly RMCReagentSystem _reagent = null!;

    private readonly ReagentId[] _canTick =
    [
        new ("MCNeurotoxin", null),
        new ("MCNanoMachines", null),
        new ("MCSynaptizine", null),
    ];

    private readonly List<EntityUid> _updated = [];

    private void UpdateExtension(float _)
    {
        _updated.Clear();
    }

    private void ClearTickMetabolize(EntityUid uid, Solution solution, Entity<MetabolizerComponent, OrganComponent?, SolutionContainerManagerComponent?> ent)
    {
        if (!TryTick(uid, solution, out var tickerComponent))
            return;

        if (!tickerComponent.Entries.TryGetValue(solution, out var entries))
            return;

        foreach (var entry in entries)
        {
            if (entry.Ticks != 0)
                OnReagentFinished(uid, ent, solution, entry.Reagent);

            entry.Ticks = 0;
        }
    }

    private void BeforeMetabolize(EntityUid uid, Solution solution, Entity<MetabolizerComponent, OrganComponent?, SolutionContainerManagerComponent?> ent)
    {
        if (!TryTick(uid, solution, out var tickerComponent))
            return;

        if (!tickerComponent.Entries.TryGetValue(solution, out var entries))
        {
            entries = [];
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
                if (entry.Ticks != 0)
                    OnReagentFinished(uid, ent, solution, entry.Reagent);

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

    private void OnReagentFinished(EntityUid uid, Entity<MetabolizerComponent, OrganComponent?, SolutionContainerManagerComponent?> ent, Solution solution, ReagentId reagentId)
    {
        if (!_reagent.TryIndex(reagentId, out var prototype) || prototype.Metabolisms is not { } effectsEntry)
            return;

        foreach (var (_, entry) in effectsEntry)
        {
            foreach (var effect in entry.Effects)
            {
                if (effect is not MCReagentEffect mcEffect)
                    continue;

                var actualEntity = ent.Comp2?.Body ?? uid;
                var args = new EntityEffectReagentArgs(actualEntity, EntityManager, ent, solution, FixedPoint2.Zero, prototype, null, 0);
                mcEffect.EffectFinished(args, solution, prototype);
            }
        }
    }
}
