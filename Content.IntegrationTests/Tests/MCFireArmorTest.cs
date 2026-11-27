using Content.Server.Atmos.EntitySystems;
using Content.Shared._MC.Armor.Core;
using Content.Shared._MC.Armor.Core.Components;
using Content.Shared._RMC14.Atmos;
using Content.Shared.Atmos.Components;
using Content.Shared.Damage;
using Content.Shared.Inventory;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class MCFireArmorTest
{
    [Test]
    public async Task TileFireUsesBarricadeArmorOnIgnition()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Map = "MCTalos",
            NoLoadTestPrototypes = true,
        });
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        var server = pair.Server;
        var entities = server.EntMan;

        await server.WaitAssertion(() =>
        {
            var barricade = entities.SpawnEntity("MCBarricadeMetal", map.GridCoords);
            var control = entities.SpawnEntity("MCBarricadeMetal", map.GridCoords);
            entities.RemoveComponent<MCArmorComponent>(control);

            entities.SpawnEntity("MCFire", map.GridCoords);
            server.System<SharedRMCFlammableSystem>().Update(1f / 60f);

            var unarmoredDamage = entities.GetComponent<DamageableComponent>(control).TotalDamage;
            var armoredDamage = entities.GetComponent<DamageableComponent>(barricade).TotalDamage;

            Assert.That(entities.GetComponent<FlammableComponent>(barricade).OnFire, Is.True);
            Assert.That(unarmoredDamage.Value, Is.Positive);
            Assert.That(armoredDamage.Value, Is.EqualTo(unarmoredDamage.Value * 0.2).Within(1));
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task BurningUsesInnateAndWornFireArmor()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Map = "MCTalos",
            NoLoadTestPrototypes = true,
        });
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        var server = pair.Server;
        var entities = server.EntMan;

        await server.WaitAssertion(() =>
        {
            var barricade = entities.SpawnEntity("MCBarricadeMetal", map.GridCoords);
            var control = entities.SpawnEntity("MCBarricadeMetal", map.GridCoords);
            entities.RemoveComponent<MCArmorComponent>(control);

            var wearer = entities.SpawnEntity("MCMobHuman", map.GridCoords);
            var armor = entities.SpawnEntity("MCArmorModularJaegerScout", map.GridCoords);
            var armorComponent = entities.GetComponent<MCArmorComponent>(armor);
            armorComponent.Soft = new MCArmorDefinition { Fire = 100, Bullet = 40 };
            armorComponent.Hard = new MCArmorDefinition { Fire = 12, Bullet = 7 };

            Assert.That(server.System<InventorySystem>().TryEquip(wearer, armor, "outerClothing", force: true), Is.True);
            Assert.That(server.System<MCArmorSystem>().TryGetArmor(wearer, out var soft, out var hard), Is.True);
            Assert.That(soft.Fire, Is.EqualTo(100));
            Assert.That(hard.Fire, Is.EqualTo(12));
            Assert.That(soft.Bullet, Is.EqualTo(40));
            Assert.That(hard.Bullet, Is.EqualTo(7));

            var fire = server.System<SharedRMCFlammableSystem>();
            foreach (var target in new[] { barricade, control, wearer })
            {
                Assert.That(fire.Ignite(target, 30, 20, maxStacks: null), Is.True);
                Assert.That(entities.GetComponent<FlammableComponent>(target).OnFire, Is.True);
            }

            server.System<FlammableSystem>().Update(1f);

            Assert.That(entities.GetComponent<DamageableComponent>(control).TotalDamage.Float(), Is.EqualTo(22.5f));
            var armoredDamage = entities.GetComponent<DamageableComponent>(barricade).TotalDamage;
            Assert.That(armoredDamage.Double(), Is.InRange(4.49, 4.5));
            Assert.That(entities.GetComponent<DamageableComponent>(wearer).TotalDamage.Float(), Is.Zero);
        });

        await pair.CleanReturnAsync();
    }
}
