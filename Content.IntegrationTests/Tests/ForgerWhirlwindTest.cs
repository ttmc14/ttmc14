using System.Linq;
using System.Numerics;
using Content.Shared._MC.Xeno.Abilities.Forger.Whirlwind;
using Content.Shared._MC.Xeno.Abilities.Forger.Whirlwind.Components;
using Content.Shared._MC.Xeno.Abilities.Forger.Whirlwind.Events;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Atmos;
using Content.Shared._RMC14.Map;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Damage;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class ForgerWhirlwindTest
{
    [TestCase(null)]
    [TestCase("MCWallRegular")]
    [TestCase("MCBarricadeMetal")]
    [TestCase("MCBarricadePlasteel")]
    public async Task FlightAndContact(string? obstacle)
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { NoLoadTestPrototypes = true });
        var map = await pair.CreateTestMap();
        var server = pair.Server;
        var entities = server.EntMan;
        await server.WaitAssertion(() =>
        {
            var mapSystem = server.System<SharedMapSystem>();
            for (var x = 0; x < 9; x++)
            for (var y = 0; y < 3; y++)
                mapSystem.SetTile(map.Grid, map.Grid.Comp, new Vector2i(x, y), map.Tile.Tile);

            var start = new EntityCoordinates(map.Grid, 0.25f, 0.92f);
            var aim = new Vector2(0.345f, 0.034f);
            var target = start.Offset(aim);
            var creator = entities.SpawnEntity(null, start);
            var victim = entities.SpawnEntity("MCMobHuman", new EntityCoordinates(map.Grid, 0.9f, 1.27f));

            EntityUid? obstacleUid = null;
            if (obstacle != null)
                obstacleUid = entities.SpawnEntity(obstacle, new EntityCoordinates(map.Grid, 2.5f, 1.5f));

            var system = server.System<MCXenoWhirlwindProjectileSystem>();
            var uid = system.Launch("MCXenoProjectileWhirlwind", creator, target);
            var transform = entities.GetComponent<TransformComponent>(uid);

            // At least one of two consecutive 60 Hz frames falls between flight updates.
            system.Update(1f / 60f);
            var firstFrame = transform.LocalPosition;
            system.Update(1f / 60f);
            Assert.That(firstFrame == start.Position || firstFrame == transform.LocalPosition, Is.True);

            // Advance just the whirlwind to distinguish its contact damage from fire on the floor.
            system.Update(2f);

            var end = transform.LocalPosition;
            var damage = entities.GetComponent<DamageableComponent>(victim);
            Assert.That(entities.IsQueuedForDeletion(uid), Is.True);
            Assert.That(damage.TotalDamage.Float(), Is.EqualTo(50));
            var fires = server.System<RMCMapSystem>();
            Assert.That(fires.HasAnchoredEntityEnumerator<TileFireComponent>(new EntityCoordinates(map.Grid, 1.5f, 1.5f)), Is.True);

            if (obstacleUid is { } hit)
            {
                Assert.That(end.X, Is.InRange(1.8f, 2f));

                if (obstacle is "MCBarricadeMetal" or "MCBarricadePlasteel")
                    Assert.That(entities.GetComponent<DamageableComponent>(hit).TotalDamage.Double(), Is.InRange(9.99, 10.0));

                return;
            }

            Assert.That(Vector2.Distance(end, start.Position + Vector2.Normalize(aim) * 8), Is.LessThan(0.0001f));
        });

        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task FarClickOnWallClampsFlight()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { NoLoadTestPrototypes = true });
        var map = await pair.CreateTestMap();
        var server = pair.Server;
        var entities = server.EntMan;
        var start = new EntityCoordinates(map.MapUid, -40.25f, 0.25f);
        var target = new EntityCoordinates(map.Grid, 0.5f, 0.5f);

        await server.WaitAssertion(() =>
        {
            entities.SpawnEntity("MCWallRegular", target);
            var forger = entities.SpawnEntity("MCXenoForger", start);
            var action = server.System<SharedRMCActionsSystem>().GetActionsWithEvent<MCXenoWhirlwindActionEvent>(forger).Single();
            var actions = server.System<SharedActionsSystem>();
            var worldTarget = entities.GetComponent<WorldTargetActionComponent>(action);
            Assert.That(actions.ValidateWorldTarget(forger, target, (action, worldTarget)), Is.True);
            actions.PerformAction(forger, action, new MCXenoWhirlwindActionEvent { Target = target });
        });

        await pair.RunSeconds(0.9f);
        await server.WaitAssertion(() =>
        {
            var query = entities.AllEntityQueryEnumerator<MCXenoWhirlwindProjectileComponent>();
            Assert.That(query.MoveNext(out var uid, out _), Is.True, "The far click should have launched the whirlwind.");
            var system = server.System<MCXenoWhirlwindProjectileSystem>();
            for (var i = 0; i < 120 && !entities.IsQueuedForDeletion(uid); i++)
                system.Update(1f / 60f);

            var transform = server.System<SharedTransformSystem>();
            var direction = Vector2.Normalize(transform.ToMapCoordinates(target).Position - start.Position);
            var expected = start.Position + direction * 8;
            Assert.That(entities.IsQueuedForDeletion(uid), Is.True);
            Assert.That(Vector2.Distance(transform.GetMapCoordinates(uid).Position, expected), Is.LessThan(0.001f));
        });

        await pair.CleanReturnAsync();
    }
}
