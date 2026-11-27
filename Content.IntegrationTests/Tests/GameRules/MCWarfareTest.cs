using System.Linq;
using Content.Server._MC.Xeno.Hive;
using Content.Server._MC.Xeno.Spawn;
using Content.Server._RMC14.Xenonids.Hive;
using Content.Server.GameTicking;
using Content.Server.Station.Components;
using Content.Server.Voting.Managers;
using Content.Shared._MC.Rules.Common;
using Content.Shared._MC.Rules.EndRound;
using Content.Shared._MC.Rules.Warfare;
using Content.Shared._MC.Xeno.Constructions.Silo;
using Content.Shared._MC.Xeno.Hive.Components;
using Content.Shared._MC.Xeno.Hive.Join;
using Content.Shared._RMC14.Marines;
using Content.Shared._RMC14.Rules;
using Content.Shared.CCVar;
using Content.Shared.Shuttles.Components;
using Robust.Shared;
using Robust.Shared.GameObjects;

namespace Content.IntegrationTests.Tests.GameRules;

[TestFixture]
public sealed class MCWarfareTest
{
    [Test]
    public async Task DeploymentUsesReservesAndEndsWhenTheLastSiloIsLost()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            InLobby = true,
            Dirty = true,
            Map = "MCCanterbury",
            NoLoadTestPrototypes = true,
        });
        var server = pair.Server;
        var entities = server.EntMan;
        var ticker = server.System<GameTicker>();
        var hives = server.System<MCXenoHiveSystem>();
        var votes = server.ResolveDependency<IVoteManager>();
        await server.WaitPost(() =>
        {
            // Both maps are fixed for this test. Close the lobby ballots before adding an offline session.
            foreach (var vote in votes.ActiveVotes.ToArray())
            {
                vote.Cancel();
            }
            votes.Update();
        });
        EntityUid secondaryHive = default;
        var marine = await server.AddDummySession();
        await pair.RunTicksSync(5);

        await server.WaitAssertion(() =>
        {
            server.CfgMan.SetCVar(CVars.NetPVS, true);
            server.CfgMan.SetCVar(CCVars.FTLArrivalTime, 0.1f);
            var planet = server.System<RMCPlanetSystem>().GetAllPlanets().Single(map => map.Proto.ID == "MCPlanetIceCaves");
            server.System<MCXenoSpawnSystem>().SetPlanet(planet);
            ticker.SetGamePreset("MCWarfare", force: true);
            Assert.That(ticker.RunLevel, Is.EqualTo(GameRunLevel.InRound));
        });
        await pair.RunTicksSync(2);

        await server.WaitAssertion(() =>
        {
            var ruleId = ticker.GetActiveGameRules().Single(id => entities.HasComponent<MCRuleWarfareComponent>(id));
            var rule = entities.GetComponent<MCRuleReinforcementsComponent>(ruleId);
            var hiveId = hives.DefaultHive!.Value;
            var hive = entities.GetComponent<MCXenoHiveComponent>(hiveId);
            var planet = entities.GetComponent<MCRuleXenoMapComponent>(ruleId).Map!.Value;
            var shuttle = entities.AllEntities<FTLComponent>().Single(ship => entities.HasComponent<AlmayerComponent>(ship));
            var ftl = shuttle.Comp;

            Assert.That(entities.GetComponent<TransformComponent>(ftl.TargetCoordinates.EntityId).GridUid, Is.EqualTo(planet));
            Assert.That(ftl.TravelTime, Is.EqualTo(600));
            Assert.That(entities.GetComponent<MCRuleHiveCollapseAfterLandingComponent>(ruleId).Landed, Is.False);
            Assert.That(hive.Configuration.General.CollapseTime.Keys, Is.EquivalentTo(new[] { MCXenoHiveCollapseType.Silo }));
            Assert.That(rule.MarineReserve, Is.EqualTo(60));
            Assert.That(rule.XenoReserve, Is.EqualTo(20));
            Assert.That(hive.BurrowedLarva, Is.EqualTo(1));

            var station = entities.AllEntities<StationJobsComponent>().Single().Owner;
            rule.MarineReserve = 0;
            ticker.MakeJoinGame(marine, station, "MCSquadMarine");
            Assert.That(marine.AttachedEntity, Is.Null);

            rule.MarineReserve = 1;
            ticker.MakeJoinGame(marine, station, "MCSquadMarine");
            Assert.That(marine.AttachedEntity, Is.Not.Null);
            Assert.That(rule.MarineReserve, Is.Zero);

            secondaryHive = server.System<XenoHiveSystem>().CreateHive("Secondary hive", "MCXenoHive");
            entities.GetComponent<MCXenoHiveComponent>(secondaryHive).Configuration.General.AllowCollapse = true;
        });

        await pair.Client.WaitPost(() => pair.Client.EntMan.EntityNetManager.SendSystemNetworkMessage(new MCJoinLobbyLarvaEvent()));
        await pair.RunTicksSync(5);

        await server.WaitAssertion(() =>
        {
            var hiveId = hives.DefaultHive!.Value;
            var player = server.PlayerMan.GetSessionById(pair.Client.Session!.UserId);
            Assert.That(ticker.UserHasJoinedGame(player), Is.True);
            Assert.That(player.AttachedEntity.HasValue && hives.IsMember(player.AttachedEntity.Value, hiveId), Is.True);
            Assert.That(hives.GetBurrowedLarva(hiveId), Is.Zero);

            var silos = entities.AllEntities<MCXenoSiloComponent>().Where(silo => hives.IsMember(silo.Owner, hiveId)).ToArray();
            Assert.That(silos, Is.Not.Empty);
            foreach (var silo in silos)
            {
                entities.DeleteEntity(silo.Owner);
            }

            // Skip the transit wait while exercising the shuttle's normal landing sequence.
            var ftl = entities.AllEntities<FTLComponent>().Single(ship => entities.HasComponent<AlmayerComponent>(ship)).Comp;
            ftl.TravelTime = 0.2f;
            ftl.StateTime = default;
        });

        await pair.RunSeconds(11);
        await server.WaitAssertion(() =>
        {
            var hive = entities.GetComponent<MCXenoHiveComponent>(hives.DefaultHive!.Value);
            Assert.That(entities.AllEntities<MCRuleHiveCollapseAfterLandingComponent>().Single().Comp.Landed, Is.True);
            Assert.That(entities.GetComponent<MCXenoHiveComponent>(secondaryHive).Collapse, Is.Empty);
            Assert.That(hive.Collapse.Keys, Is.EquivalentTo(new[] { MCXenoHiveCollapseType.Silo }));
            hive.Collapse[MCXenoHiveCollapseType.Silo] = server.Timing.CurTime;
        });
        await pair.RunSeconds(11);
        await server.WaitAssertion(() =>
        {
            Assert.That(ticker.RunLevel, Is.EqualTo(GameRunLevel.PostRound));
            var result = entities.AllEntities<MCRuleEndRoundComponent>().Single().Comp.Result;
            Assert.That(result, Is.EqualTo("MajorMarineVictory"));
        });

        await pair.CleanReturnAsync();
    }
}
