using Content.Shared._MC.Chemistry.Solutions.Effects;
using Content.Shared._MC.Chemistry.Solutions.EventProvider.Components;
using Content.Shared._RMC14.Chemistry.Reagent;
using Content.Shared.Chemistry.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;

namespace Content.Shared._MC.Chemistry.Solutions.EventProvider;

public sealed class MCSolutionEventProviderSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = null!;

    [Dependency] private readonly RMCReagentSystem _rmcReagent = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCSolutionEventProviderComponent, DamageChangedEvent>(OnDamageChanged);
    }

    private void OnDamageChanged(Entity<MCSolutionEventProviderComponent> entity, ref DamageChangedEvent args)
    {
        var damage = args.DamageDelta ?? new DamageSpecifier();
        Provide((entity, entity), (effect, solution, reagent) => effect.ProcessDamaged(entity, solution, reagent, damage));
    }

    private void Provide(Entity<MCSolutionEventProviderComponent?> entity, Action<MCReagentEffect, Solution, ReagentPrototype> callback)
    {
        if (!Resolve(entity, ref entity.Comp))
            return;

        if (!_solution.TryGetSolution(entity.Owner, entity.Comp.Solution, out _, out var solution))
            return;

        foreach (var reagentId in entity.Comp.AllowedReagents)
        {
            if (!solution.ContainsReagent(reagentId))
                continue;

            if (!_rmcReagent.TryIndex(reagentId, out var reagent) || reagent.Metabolisms is null)
                continue;

            foreach (var (_, reactiveEffect) in reagent.Metabolisms)
            {
                foreach (var effect in reactiveEffect.Effects)
                {
                    if (effect is not MCReagentEffect reagentEffect)
                        continue;

                    callback.Invoke(reagentEffect, solution, reagent);
                }
            }
        }
    }
}
