using System.Numerics;
using Content.Client.Clickable;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Interaction;
using Content.Shared.Weapons.Melee;
using Robust.Client.GameObjects;
using Robust.Shared.GameObjects;
using Robust.Shared.Graphics;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class MCStructureMeleeTest
{
    [Test]
    public async Task EmptyHandLightAttackDamagesEquipment()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Map = "MCTalos",
            Connected = true,
            NoLoadTestPrototypes = true,
        });
        var (server, client) = pair;
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        var melee = server.System<SharedMeleeWeaponSystem>();
        var interaction = client.System<SharedInteractionSystem>();
        var combat = server.System<SharedCombatModeSystem>();

        foreach (var prototype in new[] { "MCRazorwireDeployed", "MCASRSTeleporterPad", "MCASRSExportPad" })
        {
            EntityUid attacker = default;
            EntityUid target = default;
            await server.WaitPost(() =>
            {
                target = server.EntMan.SpawnEntity(prototype, new EntityCoordinates(map.Grid, 0.5f, 0.5f));
                if (prototype == "MCASRSExportPad")
                    server.System<SharedTransformSystem>().AnchorEntity(target);
                attacker = server.EntMan.SpawnEntity("MCXenoRunner", new EntityCoordinates(map.Grid, -0.5f, 0.5f));
                combat.SetInCombatMode(attacker, true);
            });
            await pair.RunTicksSync(5);
            var clientTarget = pair.ToClientUid(target);
            var clientAttacker = pair.ToClientUid(attacker);
            await client.WaitAssertion(() =>
            {
                Assert.That(interaction.CombatModeCanHandInteract(clientAttacker, clientTarget), Is.False, prototype);
                if (prototype == "MCRazorwireDeployed")
                    return;

                var sprite = client.EntMan.GetComponent<SpriteComponent>(clientTarget);
                var position = client.System<SharedTransformSystem>().GetWorldPosition(clientTarget);
                var click = position + new Vector2(0, -0.25f);
                Assert.That(client.System<ClickableSystem>().CheckClick((clientTarget, null, sprite, null, null),
                    click, new Eye(), true, out _, out _, out _), Is.True, prototype);
            });
            await server.WaitAssertion(() =>
            {
                Assert.That(melee.TryGetWeapon(attacker, out var weaponUid, out var weapon), Is.True);
                Assert.That(melee.AttemptLightAttack(attacker, weaponUid, weapon, target), Is.True, prototype);
                Assert.That(server.EntMan.GetComponent<DamageableComponent>(target).TotalDamage.Float(), Is.Positive, prototype);

                server.EntMan.DeleteEntity(target);
                server.EntMan.DeleteEntity(attacker);
            });
        }

        await server.WaitPost(() => server.System<SharedMapSystem>().DeleteMap(map.MapId));
        await pair.CleanReturnAsync();
    }
}
