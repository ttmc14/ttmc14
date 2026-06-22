using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared._MC.Armor.Core;
using Content.Shared.Damage;
using Content.Shared.Effects;
using Content.Shared.Inventory;
using Robust.Shared.Player;

namespace Content.Shared._MC.ZLevels;

public sealed class MCZLevelFallDamageSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly MCArmorSystem _mcArmor = null!;
    [Dependency] private readonly SharedColorFlashEffectSystem _color = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCZLevelFallDamageComponent, CEZLevelHitEvent>(OnHit);
    }

    private void OnHit(Entity<MCZLevelFallDamageComponent> entity, ref CEZLevelHitEvent args)
    {
        var damage = entity.Comp.Damage * args.ImpactPower * args.ImpactPower;

        _mcArmor.TryGetArmor(entity.Owner, out var soft, out var hard, SlotFlags.OUTERCLOTHING | SlotFlags.INNERCLOTHING | SlotFlags.FEET);
        damage *= MCArmorSystem.ArmorToValue(soft.Fall, hard.Fall, 0, 1, damage.GetTotal().Float());

        _damageable.TryChangeDamage(entity, damage, true);
        _color.RaiseEffect(Color.Red, new List<EntityUid> { entity }, Filter.Pvs(entity, entityManager: EntityManager));
    }
}
