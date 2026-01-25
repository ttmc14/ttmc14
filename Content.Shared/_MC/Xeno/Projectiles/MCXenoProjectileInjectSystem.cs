using Content.Shared._MC.Armor;
using Content.Shared._MC.Xeno.Abilities;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Projectiles;

namespace Content.Shared._MC.Xeno.Projectiles;

public sealed class MCXenoProjectileInjectSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedSolutionContainerSystem _solution = null!;
    [Dependency] private readonly MCArmorSystem _mcArmor = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoProjectileInjectComponent, ProjectileHitEvent>(OnHit);
    }

    private void OnHit(Entity<MCXenoProjectileInjectComponent> entity, ref ProjectileHitEvent args)
    {
        if (!_solution.TryGetSolution(args.Target, entity.Comp.Solution, out var solution, out _))
            return;

        var armor = MCArmorSystem.ArmorToValue(_mcArmor.GetSoftArmor(args.Target, SlotFlags.HEAD)?.Bio ?? 0);
        foreach (var reagentQuantity in entity.Comp.Reagents)
        {
            _solution.TryAddReagent(solution.Value, reagentQuantity.Reagent, reagentQuantity.Quantity * armor, out _);
        }

        if (!entity.Comp.Effect)
            return;

        RaiseEffect(entity, args.Target, entity.Comp.EffectColor);
    }
}
