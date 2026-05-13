namespace Content.Shared._MC.Medical.Defibrillator;

public sealed partial class MCDefibrillatorSystem
{
    // NOTE: How much do we really need this system,
    // or is it just a relic of the past RMC?

#if ENABLED
    [Dependency] private readonly SharedRMCBloodstreamSystem _rmcBloodstream = null!;
    [Dependency] private readonly RMCReagentSystem _rmcReagent = null!;

    [Dependency] private readonly SharedSolutionContainerSystem _solutionContainer = null!;


    private float GetSolutionHealAmount(Entity<DefibrillatorComponent> entity, EntityUid target)
    {
        if (!_rmcBloodstream.TryGetChemicalSolution(target, out var solutionEntity, out _))
            return 0;

        (Reagent Reagent, FixedPoint2 Heal, Electrogenetic Electrogenetic)? highest = null;
        foreach (var quantity in solutionEntity.Comp.Solution.Contents)
        {
            if (!_rmcReagent.TryIndex(quantity.Reagent.Prototype, out var reagent))
                continue;

            if (reagent.Metabolisms is null || !reagent.Metabolisms.TryGetValue(entity.Comp.MetabolismId, out var effects))
                continue;

            foreach (var effect in effects.Effects)
            {
                if (effect is not Electrogenetic electrogenetic)
                    continue;

                if (highest is null || electrogenetic.HealAmount > highest.Value.Heal)
                    highest = (reagent, electrogenetic.HealAmount, electrogenetic);
            }
        }

        if (highest is null)
            return 0;

        var heal = highest.Value.Electrogenetic.CalculateHeal(_damageable, target, EntityManager);
        _solutionContainer.RemoveReagent(solutionEntity, highest.Value.Reagent.ID, 1);

        return heal.GetTotal().Float();
    }
#endif

}
