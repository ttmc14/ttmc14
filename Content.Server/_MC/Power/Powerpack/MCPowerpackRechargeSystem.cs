using Content.Shared._MC.Power.Battery;
using Content.Shared._MC.Power.Battery.Components;
using Content.Shared._MC.Power.Battery.Events;
using Content.Shared._MC.Power.Powerpack.Components;
using Content.Shared._MC.Weapon.Laser.Components;
using Content.Shared._MC.Weapon.Laser.Systems;
using Content.Shared._RMC14.Hands;
using Content.Shared.Inventory;
using Robust.Shared.Containers;

namespace Content.Server._MC.Power.Powerpack;

public sealed class MCPowerpackRechargeSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _containers = null!;
    [Dependency] private readonly InventorySystem _inventory = null!;
    [Dependency] private readonly RMCHandsSystem _hands = null!;
    [Dependency] private readonly MCWeaponLaserSystem _laser = null!;

    public override void Initialize() =>
        SubscribeLocalEvent<MCWeaponLaserAmmoComponent, MCBatteryChargeChangedEvent>(OnChargeChanged,
            after: [typeof(MCBatteryVisualsSystem)]);

    private void OnChargeChanged(Entity<MCWeaponLaserAmmoComponent> cell, ref MCBatteryChargeChangedEvent args)
    {
        if (args.Charge == args.PreviousCharge || args.Charge >= args.MaxCharge)
            return;

        if (!_containers.TryGetContainingContainer(cell.Owner, out var slot) ||
            !TryComp<MCWeaponLaserComponent>(slot.Owner, out var laser) ||
            slot.ID != laser.ContainerId ||
            !_hands.TryGetHolder(slot.Owner, out var holder))
            return;

        var slots = _inventory.GetSlotEnumerator(holder, SlotFlags.BACK);
        while (slots.NextItem(out var pack))
        {
            if (!HasComp<MCPowerpackComponent>(pack) || !TryComp<MCBatteryComponent>(pack, out var battery) || battery.Charge <= 0f)
                continue;

            _laser.Recharge((slot.Owner, laser), (pack, battery));
            return;
        }
    }
}
