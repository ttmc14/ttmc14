using Content.Shared._MC.Power.Battery;
using Content.Shared._MC.Power.Battery.Components;
using Content.Shared._MC.Power.Powerpack.Components;
using Content.Shared._MC.Weapon.Laser.Events;
using Content.Shared._MC.Weapon.Laser.Systems;
using Content.Shared.Hands;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Content.Shared.Inventory.Events;
using Robust.Shared.Network;

namespace Content.Shared._MC.Power.Powerpack;

public sealed class MCPowerpackRechargeSystem : EntitySystem
{
    [Dependency] private readonly SharedHandsSystem _hands = null!;
    [Dependency] private readonly InventorySystem _inventory = null!;
    [Dependency] private readonly INetManager _net = null!;

    [Dependency] private readonly MCBatterySystem _batteries = null!;
    [Dependency] private readonly MCWeaponLaserSystem _laser = null!;

    /// <summary>
    /// Tracks equipped recharge packs and handles laser cell changes for their wearers.
    /// </summary>
    public override void Initialize()
    {
        SubscribeLocalEvent<MCPowerpackRechargeComponent, ComponentStartup>(OnStartup);
        SubscribeLocalEvent<MCPowerpackRechargeComponent, ComponentShutdown>(OnShutdown);
        SubscribeLocalEvent<MCPowerpackRechargeComponent, GotEquippedEvent>(OnEquipped);
        SubscribeLocalEvent<MCPowerpackRechargeComponent, GotUnequippedEvent>(OnUnequipped);

        SubscribeLocalEvent<MCPowerpackWearerComponent, MCLaserCellChargeChangedEvent>(OnChargeChanged);
        SubscribeLocalEvent<MCPowerpackWearerComponent, MCLaserCellRemovedEvent>(OnCellRemoved);
        SubscribeLocalEvent<MCPowerpackWearerComponent, DidUnequipHandEvent>(OnDropped);
    }

    private void OnStartup(Entity<MCPowerpackRechargeComponent> entity, ref ComponentStartup args)
    {
        if (_inventory.InSlotWithFlags(entity.Owner, SlotFlags.BACK))
            Connect(entity, Transform(entity).ParentUid);
    }

    private void OnShutdown(Entity<MCPowerpackRechargeComponent> entity, ref ComponentShutdown args) =>
        Disconnect(entity);

    private void OnEquipped(Entity<MCPowerpackRechargeComponent> entity, ref GotEquippedEvent args)
    {
        if ((args.SlotFlags & SlotFlags.BACK) == 0)
            return;

        Connect(entity, args.Equipee);
    }

    private void OnUnequipped(Entity<MCPowerpackRechargeComponent> entity, ref GotUnequippedEvent args) =>
        Disconnect(entity);

    private void OnCellRemoved(Entity<MCPowerpackWearerComponent> entity, ref MCLaserCellRemovedEvent args) =>
        ForgetGun(entity, args.Gun);

    private void OnChargeChanged(Entity<MCPowerpackWearerComponent> entity, ref MCLaserCellChargeChangedEvent args)
    {
        foreach (var pack in entity.Comp.Packs)
        {
            if (!TryComp<MCPowerpackRechargeComponent>(pack, out var recharge) || !TryComp<MCBatteryComponent>(pack, out var source))
                continue;

            if (!recharge.Cells.TryGetValue(args.Gun, out var cell) || cell.Owner != args.Cell || cell.Comp.Deleted)
            {
                if (!_hands.IsHolding(entity.Owner, args.Gun) ||
                    !_laser.TryGetInstalledBattery(args.Gun, out cell) || cell.Owner != args.Cell)
                    return;

                recharge.Cells[args.Gun] = cell;
            }

            if (_batteries.TransferCharge((pack, source), cell) <= 0f)
                continue;

            args.Recharged = true;
            return;
        }
    }

    private void OnDropped(Entity<MCPowerpackWearerComponent> entity, ref DidUnequipHandEvent args) =>
        ForgetGun(entity, args.Unequipped);

    private void ForgetGun(Entity<MCPowerpackWearerComponent> entity, EntityUid gun)
    {
        foreach (var pack in entity.Comp.Packs)
        {
            if (TryComp<MCPowerpackRechargeComponent>(pack, out var recharge))
                recharge.Cells.Remove(gun);
        }
    }

    private void Connect(Entity<MCPowerpackRechargeComponent> pack, EntityUid wearer)
    {
        if (_net.IsClient)
            return;

        pack.Comp.Wearer = wearer;
        Dirty(pack);

        var relay = EnsureComp<MCPowerpackWearerComponent>(wearer);
        if (relay.Packs.Add(pack))
            Dirty(wearer, relay);
    }

    private void Disconnect(Entity<MCPowerpackRechargeComponent> pack)
    {
        pack.Comp.Cells.Clear();

        if (_net.IsClient || pack.Comp.Wearer is not { } wearer)
            return;

        pack.Comp.Wearer = null;

        if (!pack.Comp.Deleted)
            Dirty(pack);

        if (!TryComp<MCPowerpackWearerComponent>(wearer, out var relay))
            return;

        if (!relay.Packs.Remove(pack))
            return;

        if (relay.Packs.Count == 0)
        {
            RemComp<MCPowerpackWearerComponent>(wearer);
            return;
        }

        Dirty(wearer, relay);
    }
}
