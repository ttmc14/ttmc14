using Content.Shared._MC.Armor;
using Content.Shared.Chemistry.EntitySystems;
using Content.Shared.Effects;
using Content.Shared.Inventory;
using Content.Shared.Projectiles;
using Robust.Shared.Player;

namespace Content.Shared._MC.Weapons.Projectiles.Inject;

public sealed class MCProjectileInjectSystem : EntitySystem
{
    [Dependency] private readonly SharedColorFlashEffectSystem _colorFlash = null!;
    [Dependency] private readonly SharedSolutionContainerSystem _solution = null!;

    [Dependency] private readonly MCArmorSystem _mcArmor = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCProjectileInjectComponent, ProjectileHitEvent>(OnHit);
    }

    private void OnHit(Entity<MCProjectileInjectComponent> entity, ref ProjectileHitEvent args)
    {
        if (!_solution.TryGetSolution(args.Target, entity.Comp.Solution, out var solution, out _))
            return;

        var armor = GetArmor(entity, args.Target);
        foreach (var reagentQuantity in entity.Comp.Reagents)
        {
            _solution.TryAddReagent(solution.Value, reagentQuantity.Reagent, reagentQuantity.Quantity * armor, out _);
        }

        RaiseEffect(entity, args.Target);
    }

    private float GetArmor(Entity<MCProjectileInjectComponent> entity, EntityUid targetUid)
    {
        return entity.Comp.IgnoreArmor ? 1f : MCArmorSystem.ArmorToValue(_mcArmor.GetSoftArmor(targetUid, SlotFlags.HEAD)?.Bio ?? 0);
    }

    private void RaiseEffect(Entity<MCProjectileInjectComponent> entity, EntityUid targetUid)
    {
        if (!entity.Comp.Effect)
            return;

        Flash(entity, targetUid, entity.Comp.EffectColor);
    }

    private void Flash(EntityUid ownerUid, EntityUid targetUid, Color? color = null)
    {
        var filter = Filter.Pvs(targetUid, entityManager: EntityManager).RemoveWhereAttachedEntity(uid => uid == ownerUid);
        _colorFlash.RaiseEffect(color ?? Color.Red, new List<EntityUid> { targetUid }, filter);
    }
}
