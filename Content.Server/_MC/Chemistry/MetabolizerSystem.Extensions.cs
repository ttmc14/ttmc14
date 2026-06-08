using System.Diagnostics.CodeAnalysis;
using Content.Server.Body.Components;
using Content.Shared._MC.Chemistry.Solutions;
using Content.Shared._MC.Chemistry.Solutions.Effects;
using Content.Shared._MC.Chemistry.Solutions.Ticker.Components;
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
    private const string TickableSolution = "chemicals";

    [Dependency] private readonly RMCReagentSystem _reagent = null!;

    private readonly HashSet<ReagentId> _tickableReagents =
    [
        new ("MCNeurotoxin", null),
        new ("MCNanoMachines", null),
        new ("MCSynaptizine", null),
        new ("MCRussianRed", null),
        new ("MCNeuraline", null),
        new ("MCAdrenalin", null),
    ];

    private readonly HashSet<EntityUid> _updated = [];

    private void UpdateExtension(float _)
    {
        _updated.Clear();

        var ev = new MCSolutionBeforeEffectEvent();
        RaiseLocalEvent(ref ev);
    }

    private void BeforeMetabolize(
        EntityUid uid,
        Solution solution,
        Entity<MetabolizerComponent, OrganComponent?, SolutionContainerManagerComponent?> ent)
    {
        if (!TryGetTicker(uid, solution, out var ticker))
            return;

        var entries = GetOrCreateEntries(ticker, solution);
        UpdateTickEntries(uid, ent, solution, entries);
    }

    private void ClearTickMetabolize(
        EntityUid uid,
        Solution solution,
        Entity<MetabolizerComponent, OrganComponent?, SolutionContainerManagerComponent?> ent)
    {
        if (!TryGetTicker(uid, solution, out var ticker))
            return;

        if (!ticker.Entries.TryGetValue(solution, out var entries))
            return;

        foreach (var entry in entries)
        {
            if (entry.Ticks > 0)
                OnReagentFinished(uid, ent, solution, entry.Reagent);

            entry.Ticks = 0;
        }
    }

    private bool TryGetTicker(
        EntityUid uid,
        Solution solution,
        [NotNullWhen(true)] out MCSolutionTickerComponent? ticker)
    {
        ticker = null;

        if (solution.Name != TickableSolution)
            return false;

        return TryComp(uid, out ticker) && _updated.Add(uid);
    }

    private List<MCSolutionTickerComponent.TickEntry> GetOrCreateEntries(
        MCSolutionTickerComponent ticker,
        Solution solution)
    {
        if (ticker.Entries.TryGetValue(solution, out var entries))
            return entries;

        entries = [];
        ticker.Entries[solution] = entries;

        return entries;
    }

    private void UpdateTickEntries(
        EntityUid uid,
        Entity<MetabolizerComponent, OrganComponent?, SolutionContainerManagerComponent?> ent,
        Solution solution,
        List<MCSolutionTickerComponent.TickEntry> entries)
    {
        foreach (var entry in entries)
        {
            if (solution.TryGetReagent(entry.Reagent, out _))
            {
                entry.Ticks++;
                continue;
            }

            if (entry.Ticks <= 0)
                continue;

            OnReagentFinished(uid, ent, solution, entry.Reagent);
            entry.Ticks = 0;
        }

        foreach (var reagent in solution.Contents)
        {
            if (!_tickableReagents.Contains(reagent.Reagent))
                continue;

            if (entries.Exists(e => e.Reagent == reagent.Reagent))
                continue;

            entries.Add(new MCSolutionTickerComponent.TickEntry(reagent.Reagent, 1));
        }
    }

    private void OnReagentFinished(
        EntityUid uid,
        Entity<MetabolizerComponent, OrganComponent?, SolutionContainerManagerComponent?> ent,
        Solution solution,
        ReagentId reagentId)
    {
        if (!_reagent.TryIndex(reagentId, out var prototype))
            return;

        if (prototype.Metabolisms is not { } effectsEntry)
            return;

        var actualEntity = ent.Comp2?.Body ?? uid;
        var args = new EntityEffectReagentArgs(
            actualEntity,
            EntityManager,
            ent,
            solution,
            FixedPoint2.Zero,
            prototype,
            null,
            0
        );

        foreach (var (_, entry) in effectsEntry)
        {
            foreach (var effect in entry.Effects)
            {
                if (effect is MCReagentEffect mcEffect)
                    mcEffect.EffectFinished(args, solution, prototype);
            }
        }
    }
}
