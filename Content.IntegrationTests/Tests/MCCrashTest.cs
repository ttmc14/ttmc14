using System.Collections.Generic;
using Content.Server.GameTicking;
using Content.Server.Shuttles.Events;
using Content.Shared._MC.Rules.Common;
using Content.Shared._MC.Rules.EndRound;
using Content.Shared._MC.Rules.Utilities;
using Content.Shared._RMC14.Marines;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Preferences;
using Robust.Shared.Network;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class MCCrashTest
{
    [Test]
    public async Task GeneratorsSpawnBeforeLandingAndMarineWipeIsResolvedOnArrival()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Map = "MCTalos", NoLoadTestPrototypes = true });
        var server = pair.Server;
        var entities = server.EntMan;
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        var coords = new EntityCoordinates(map.Grid, 0.5f, 0.5f);
        EntityUid rule = default;
        EntityUid shuttle = default;
        await server.WaitPost(() =>
        {
            rule = entities.SpawnEntity(null, coords);
            entities.EnsureComponent<GameRuleComponent>(rule);
            entities.EnsureComponent<ActiveGameRuleComponent>(rule);
            entities.EnsureComponent<MCRuleEndRoundComponent>(rule);
            entities.EnsureComponent<MCRuleEndRoundMarinesDeadComponent>(rule);
            entities.EnsureComponent<MCRuleSpawnNukeDiskGeneratorsComponent>(rule);
            shuttle = entities.SpawnEntity(null, coords);
            entities.EnsureComponent<AlmayerComponent>(shuttle);
            entities.EnsureComponent<MCRuleCrashLandingComponent>(rule).Shuttle = shuttle;
            for (var x = 0; x < 3; x++)
            {
                entities.SpawnEntity("MCSpawnNukeDiskGenerator", new EntityCoordinates(map.Grid, x + 0.5f, 0.5f));
            }

            entities.EventBus.RaiseEvent(EventSource.Local, new RulePlayerSpawningEvent([], new Dictionary<NetUserId, HumanoidCharacterProfile>(), false));
        });

        await pair.RunTicksSync(2);
        await server.WaitAssertion(() =>
        {
            Assert.That(entities.GetComponent<MCRuleSpawnNukeDiskGeneratorsComponent>(rule).Spawned, Is.True);
            var marine = entities.SpawnEntity("MobHuman", coords);
            entities.EnsureComponent<MarineComponent>(marine);
            server.System<MobStateSystem>().ChangeMobState(marine, MobState.Dead);
            var end = entities.GetComponent<MCRuleEndRoundComponent>(rule);
            Assert.That(end.Result, Is.Null.Or.Empty);

            var arrived = new FTLCompletedEvent(shuttle, map.MapUid);
            entities.EventBus.RaiseLocalEvent(shuttle, ref arrived);
            Assert.That(end.Result, Is.EqualTo("MajorXenoVictory"));
        });

        await server.WaitPost(() => server.System<SharedMapSystem>().DeleteMap(map.MapId));
        await pair.CleanReturnAsync();
    }
}
