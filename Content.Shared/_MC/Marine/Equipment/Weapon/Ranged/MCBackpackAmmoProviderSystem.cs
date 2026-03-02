using Content.Shared.Inventory;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Containers;

namespace Content.Shared._MC.Marine.Equipment.Weapon.Ranged;

public sealed class MCBackpackAmmoProviderSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = null!;
    [Dependency] private readonly InventorySystem _inventory = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCBackpackAmmoProviderComponent, TakeAmmoEvent>(Relay);
        SubscribeLocalEvent<MCBackpackAmmoProviderComponent, GetAmmoCountEvent>(Relay);
    }

    private void Relay<T>(Entity<MCBackpackAmmoProviderComponent> entity, ref T args) where T : notnull
    {
        if (!_container.TryGetContainingContainer((entity.Owner, null), out var holder))
            return;

        var inventoryEnumerator = _inventory.GetSlotEnumerator(holder.Owner, entity.Comp.Slot);
        while (inventoryEnumerator.MoveNext(out var slot))
        {
            if (slot.ContainedEntity is not { } uid || !HasComp<MCBackpackAmmoComponent>(uid))
                continue;

            RaiseLocalEvent(uid, args);
        }
    }
}
