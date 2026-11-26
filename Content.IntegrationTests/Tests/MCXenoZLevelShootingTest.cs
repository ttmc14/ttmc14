using System.Collections.Generic;
using System.Linq;
using Content.Server._CE.ZLevels.Core;
using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._CE.ZLevels.Core.Events;
using Content.Shared._MC.Smoke.Components;
using Content.Shared._MC.Xeno.Abilities.Warlock.Blast;
using Content.Shared._MC.Xeno.Spit;
using Content.Shared._RMC14.Actions;
using Content.Shared.Actions;
using Content.Shared.DoAfter;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class MCXenoZLevelShootingTest
{
    [Test]
    public async Task PsychicBlastChecksRangeAndWallsOnTheAimedFloor()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Map = "MCTalos", NoLoadTestPrototypes = true });
        var server = pair.Server;
        var entities = server.EntMan;
        var tiles = server.ResolveDependency<ITileDefinitionManager>();
        await server.WaitAssertion(() =>
        {
            var maps = server.System<SharedMapSystem>();
            var lower = maps.CreateMap(out var lowerId);
            var upper = maps.CreateMap(out var upperId);
            var levels = server.System<CEZLevelsSystem>();
            var network = levels.CreateZNetwork();
            Assert.That(levels.TryAddMapsIntoZNetwork(network, new() { [lower] = 0, [upper] = 1 }), Is.True);
            var lowerGrid = entities.EnsureComponent<MapGridComponent>(lower);
            var upperGrid = entities.EnsureComponent<MapGridComponent>(upper);
            var floor = new Tile(tiles["CMFloorPlating"].TileId);

            for (var row = 0; row < 2; row++)
            {
                for (var x = 0; x <= 25; x++)
                {
                    maps.SetTile(lower, lowerGrid, new Vector2i(x, row), floor);
                    if (x > 0)
                    {
                        maps.SetTile(upper, upperGrid, new Vector2i(x, row), floor);
                    }
                }

                var y = row + 0.5f;
                entities.SpawnEntity("MCWallRegular", new EntityCoordinates(lower, 4.5f, y));
                if (row == 1)
                {
                    entities.SpawnEntity("MCWallRegular", new EntityCoordinates(upper, 10.5f, y));
                }

                var warlock = entities.SpawnEntity("MCXenoWarlock", new EntityCoordinates(lower, 0.5f, y));
                entities.EnsureComponent<CEZLevelViewerComponent>(warlock);
                entities.EventBus.RaiseLocalEvent(warlock, new CEToggleZLevelLookUpAction());
                var action = server.System<SharedRMCActionsSystem>().GetActionsWithEvent<MCXenoBlastActionEvent>(warlock).Single();
                var aim = new EntityCoordinates(lower, 25.5f, y);
                server.System<SharedActionsSystem>().PerformAction(warlock, action, new MCXenoBlastActionEvent { Target = aim });

                var pending = entities.GetComponent<DoAfterComponent>(warlock).DoAfters.Values.Single();
                var blast = (MCXenoBlastDoAfterEvent) pending.Args.Event;
                Assert.That(blast.Target.MapId, Is.EqualTo(upperId));
                var expectedX = row == 0 ? 14.5f : 10f;
                Assert.That(blast.Target.X, Is.EqualTo(expectedX).Within(0.1f));
            }

            maps.DeleteMap(lowerId);
            maps.DeleteMap(upperId);
            entities.DeleteEntity(network);
        });
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task NeurotoxinGlobReachesAimedTileThroughOpening()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Map = "MCTalos", NoLoadTestPrototypes = true });
        var server = pair.Server;
        var entities = server.EntMan;
        var tiles = server.ResolveDependency<ITileDefinitionManager>();
        MapId lowerId = default, upperId = default;
        EntityUid network = default, glob = default;
        MapCoordinates lastPosition = default;

        await server.WaitAssertion(() =>
        {
            var maps = server.System<SharedMapSystem>();
            var lower = maps.CreateMap(out lowerId);
            var upper = maps.CreateMap(out upperId);
            var floor = new Tile(tiles["CMFloorPlating"].TileId);
            var lowerGrid = entities.EnsureComponent<MapGridComponent>(lower);
            var upperGrid = entities.EnsureComponent<MapGridComponent>(upper);
            for (var x = 0; x <= 31; x++)
            {
                maps.SetTile(lower, lowerGrid, new Vector2i(x, 0), floor);
                maps.SetTile(upper, upperGrid, new Vector2i(x, 0), floor);
            }

            var levels = server.System<CEZLevelsSystem>();
            var zNetwork = levels.CreateZNetwork();
            network = zNetwork.Owner;
            Assert.That(levels.TryAddMapsIntoZNetwork(zNetwork, new() { [lower] = 0, [upper] = 1 }), Is.True);

            var shooter = entities.SpawnEntity("MCXenoBoiler", new EntityCoordinates(lower, 0.5f, 0.5f));
            var aim = new EntityCoordinates(lower, 30.5f, 0.5f);
            var spit = server.System<MCSharedXenoSpitSystem>();
            Assert.That(spit.TryGetShotCoordinates(shooter, aim, out _, out var target), Is.True);
            Assert.That(target.MapId, Is.EqualTo(lowerId));

            maps.SetTile(upper, upperGrid, Vector2i.Zero, Tile.Empty);
            entities.EnsureComponent<CEZLevelViewerComponent>(shooter);
            entities.EventBus.RaiseLocalEvent(shooter, new CEToggleZLevelLookUpAction());
            Assert.That(spit.TryGetShotCoordinates(shooter, aim, out var origin, out target), Is.True);
            Assert.That(origin.MapId, Is.EqualTo(upperId));
            Assert.That(target, Is.EqualTo(new MapCoordinates(aim.Position, upperId)));

            glob = spit.Shoot(shooter, origin, target, "MCXenoProjectileGlobNeurotoxin", 1, Angle.Zero, 15).Single();
            maps.SetTile(upper, upperGrid, Vector2i.Zero, floor);
            Assert.That(spit.TryGetShotCoordinates(shooter, aim, out _, out _), Is.False);
        });

        await pair.RunSeconds(1f);
        await server.WaitAssertion(() =>
        {
            Assert.That(entities.Deleted(glob), Is.False);
            lastPosition = server.System<SharedTransformSystem>().GetMapCoordinates(glob);
            Assert.That(lastPosition.MapId, Is.EqualTo(upperId));
            Assert.That(lastPosition.X, Is.GreaterThan(7.5f));
        });

        var landed = false;
        for (var tick = 0; tick < pair.SecondsToTicks(3f) && !landed; tick++)
        {
            await pair.RunTicksSync(1);
            await server.WaitPost(() =>
            {
                landed = entities.Deleted(glob);
                if (!landed)
                {
                    lastPosition = server.System<SharedTransformSystem>().GetMapCoordinates(glob);
                }
            });
        }

        await server.WaitAssertion(() =>
        {
            Assert.That(landed, Is.True);
            Assert.That(lastPosition.X, Is.EqualTo(30.5f).Within(0.5f));
            var transform = server.System<SharedTransformSystem>();
            var clouds = new List<MapCoordinates>();
            var smoke = entities.AllEntityQueryEnumerator<MCSmokeComponent>();
            while (smoke.MoveNext(out var uid, out _))
            {
                var position = transform.GetMapCoordinates(uid);
                clouds.Add(position);
            }

            Assert.That(clouds, Does.Contain(new MapCoordinates(30.5f, 0.5f, upperId)));
        });

        await server.WaitPost(() =>
        {
            var maps = server.System<SharedMapSystem>();
            maps.DeleteMap(lowerId);
            maps.DeleteMap(upperId);
            entities.DeleteEntity(network);
        });
        await pair.CleanReturnAsync();
    }
}
