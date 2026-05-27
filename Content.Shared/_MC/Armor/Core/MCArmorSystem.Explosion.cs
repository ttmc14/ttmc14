using Content.Shared.Explosion;
using Content.Shared.Inventory;
using Content.Shared._MC.Armor.Core.Components;

namespace Content.Shared._MC.Armor.Core;

public sealed partial class MCArmorSystem
{
    private void OnGetExplosionResistance(Entity<MCArmorComponent> entity, ref GetExplosionResistanceEvent args)
    {
        ApplyExplosionArmor(entity.Owner, entity.Comp, ref args);
    }

    private void OnGetExplosionResistanceRelayed(Entity<MCArmorComponent> entity, ref InventoryRelayedEvent<GetExplosionResistanceEvent> args)
    {
        ApplyExplosionArmor(entity.Owner, entity.Comp, ref args.Args);
    }

    private void ApplyExplosionArmor(EntityUid owner, MCArmorComponent armor, ref GetExplosionResistanceEvent args)
    {
        var sunder = _mcXenoSunder.GetSunder(owner);
        args.DamageCoefficient *= ArmorToValue(armor.Soft.Bomb, armor.Hard.Bomb, 0, sunder);
    }
}
