using System.Linq;
using Content.Shared._MC.Power.Battery;
using Content.Shared._MC.Power.Battery.Components;
using Content.Shared._MC.Weapon.Laser.Components;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Inventory;
using Robust.Shared.Containers;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class MCPowerpackTest
{
    [Test]
    public async Task WornPowerpackRechargesHeldLaserImmediatelyOnChargeChange()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Map = "MCTalos", NoLoadTestPrototypes = true });
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        var server = pair.Server;
        var entities = server.EntMan;
        var batteries = server.System<MCBatterySystem>();
        var hands = server.System<SharedHandsSystem>();
        var inventory = server.System<InventorySystem>();
        EntityUid wearer = default, pack = default, gun = default, cell = default;
        float capacity = 0f;

        await server.WaitAssertion(() =>
        {
            wearer = entities.SpawnEntity("MCMobHuman", map.GridCoords);
            pack = entities.SpawnEntity("MCBackpackPowerpackEmpty", map.GridCoords);
            gun = entities.SpawnEntity("MCWeaponEnergyRifle", map.GridCoords);
            cell = server.System<SharedContainerSystem>().GetContainer(gun, "gun_magazine").ContainedEntities.Single();
            capacity = batteries.GetMaxCharge(cell);
            Assert.That(batteries.GetCharge(pack), Is.Zero);
            batteries.SetCharge(pack, capacity + 100f);
            Assert.That(hands.TryPickupAnyHand(wearer, gun), Is.True);
            batteries.SetCharge(cell, capacity - 200f);
            Assert.That(batteries.GetCharge(cell), Is.EqualTo(capacity - 200f));
            Assert.That(inventory.TryEquip(wearer, pack, "back", force: true), Is.True);
        });

        await pair.RunSeconds(2f);
        await server.WaitAssertion(() =>
        {
            batteries.SetCharge(cell, capacity - 200f);
            Assert.That(batteries.GetCharge(cell), Is.EqualTo(capacity - 200f));
            Assert.That(batteries.GetCharge(pack), Is.EqualTo(capacity + 100f));

            batteries.SetCharge(cell, 0f);
            Assert.That(batteries.GetCharge(cell), Is.EqualTo(capacity));
            Assert.That(batteries.GetCharge(pack), Is.EqualTo(100f));
            var appearance = server.System<SharedAppearanceSystem>();
            Assert.That(appearance.TryGetData<int>(cell, MCBatteryVisuals.Layer, out var layer), Is.True);
            Assert.That(layer, Is.EqualTo(entities.GetComponent<MCBatteryVisualsComponent>(cell).Levels));

            Assert.That(hands.TryDrop(wearer, gun), Is.True);
            batteries.SetCharge(cell, 0f);
            Assert.That(batteries.GetCharge(cell), Is.Zero);
            Assert.That(batteries.GetCharge(pack), Is.EqualTo(100f));

            Assert.That(hands.TryPickupAnyHand(wearer, gun), Is.True);
            batteries.SetCharge(cell, 20f);
            Assert.That(batteries.GetCharge(cell), Is.EqualTo(120f));
            Assert.That(batteries.GetCharge(pack), Is.Zero);
            var laser = entities.GetComponent<MCWeaponLaserComponent>(gun);
            Assert.That(laser.Shots, Is.EqualTo((int) (120f / (capacity / laser.Capacity))));
        });

        await pair.CleanReturnAsync();
    }
}
