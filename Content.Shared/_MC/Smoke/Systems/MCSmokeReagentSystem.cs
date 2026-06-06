using Content.Shared._MC.Armor.Core;
using Content.Shared._MC.Smoke.Components;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Inventory;

namespace Content.Shared._MC.Smoke.Systems;

public sealed class MCSmokeReagentSystem : EntitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = null!;

    [Dependency] private readonly MCArmorSystem _mcArmor = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCSmokeReagentComponent, MCSmokeEffectEvent>(OnEffect);
    }

    private void OnEffect(Entity<MCSmokeReagentComponent> entity, ref MCSmokeEffectEvent args)
    {
        if (!_solution.TryGetSolution(args.TargetUid, entity.Comp.Solution, out var solution, out _))
            return;

        var armor = MCArmorSystem.ArmorToValue(_mcArmor.GetSoftArmor(args.TargetUid, SlotFlags.HEAD)?.Bio ?? 0);
        foreach (var reagentQuantity in entity.Comp.Reagents)
        {
            _solution.TryAddReagent(solution.Value, reagentQuantity.Reagent, reagentQuantity.Quantity * armor, out _);
        }
    }
}
