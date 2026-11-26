using System.Linq;
using Content.Client._MC.Beacon.UI;
using Content.Client._RMC14.TacticalMap;
using Content.Shared._MC.Areas;
using Content.Shared._MC.Engineering.Beacon;
using Content.Shared._MC.Engineering.Deploy.Components;
using Content.Shared._MC.Engineering.Deploy.Events;
using Content.Shared._MC.SerialNumber;
using Content.Shared._RMC14.Areas;
using Content.Shared.DoAfter;
using Content.Shared.Examine;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class MCBeaconTest
{
    [Test]
    public async Task SupplyBeaconsWithSameLabelKeepDistinctSerialNumbersAndDestinations()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Map = "MCTalos",
            Connected = true,
            NoLoadTestPrototypes = true,
        });
        var server = pair.Server;
        var entities = server.EntMan;
        var map = await pair.CreateTestMap(initialized: false, tile: "CMFloorPlating");
        MCBeaconSystem.NetBeaconWithName[] targets = [];
        await server.WaitAssertion(() =>
        {
            var mapSystem = server.System<SharedMapSystem>();
            mapSystem.SetTile(map.Grid, map.Grid.Comp, new Vector2i(1, 0), map.Tile.Tile);
            mapSystem.SetTile(map.Grid, map.Grid.Comp, new Vector2i(1, 1), map.Tile.Tile);
            entities.EnsureComponent<AreaGridComponent>(map.Grid);
            mapSystem.InitializeMap(map.MapId);
            var coords = new EntityCoordinates(map.Grid, 0.5f, 0.5f);
            var user = entities.SpawnEntity("MobHuman", coords);
            var first = entities.SpawnEntity("MCBeaconSupply", coords);
            var second = entities.SpawnEntity("MCBeaconSupply", coords);
            var serial = entities.GetComponent<MCSerialNumberComponent>(first).Number;
            Assert.That(serial, Does.Match("^02(:[0-9A-F]{2}){5}$"));
            Assert.That(entities.GetComponent<MCSerialNumberComponent>(second).Number, Is.Not.EqualTo(serial));

            foreach (var beacon in new[] { first, second })
            {
                var drop = beacon == first ? coords : new EntityCoordinates(map.Grid, 1.5f, 1.5f);
                var deploy = new MCDeployDoAfterEvent(entities.GetNetCoordinates(drop), Angle.Zero);
                deploy.DoAfter = new(0, new DoAfterArgs(entities, user, 0, deploy, beacon), TimeSpan.Zero);
                entities.EventBus.RaiseLocalEvent(beacon, deploy);
            }

            var name = entities.GetComponent<MetaDataComponent>(user).EntityName;
            var area = server.System<MCAreasSystem>().GetAreaName(first);
            var destinations = server.System<MCBeaconSystem>().GetNetActiveBeaconsWithName("Supply");
            targets = destinations.Where(entry => entry.Name == $"{name} + {area}").ToArray();
            Assert.That(targets.Select(entry => entry.Uid),
                Is.EquivalentTo(new[] { entities.GetNetEntity(first), entities.GetNetEntity(second) }));
            Assert.That(targets.Select(entry => entry.Grid), Is.All.EqualTo(entities.GetNetEntity(map.Grid)));
            Assert.That(targets.Single(entry => entry.Uid == entities.GetNetEntity(second)).Tile, Is.EqualTo(Vector2i.One));
            Assert.That(server.System<ExamineSystemShared>().GetExamineText(first, user).ToString(),
                Does.Contain(serial));

            var automated = new MCDeployChangedStateEvent(MCDeployState.Deployed, MCDeployState.Item);
            entities.EventBus.RaiseLocalEvent(first, ref automated);
            destinations = server.System<MCBeaconSystem>().GetNetActiveBeaconsWithName("Supply");
            Assert.That(destinations.Single(entry => entry.Uid == entities.GetNetEntity(first)).Name, Is.EqualTo(area));
            Assert.That(entities.GetComponent<MCSerialNumberComponent>(first).Number, Is.EqualTo(serial));
        });

        await pair.RunTicksSync(20);
        await server.WaitAssertion(() =>
            Assert.That(entities.GetComponent<AreaGridComponent>(map.Grid).Colors, Contains.Key(Vector2i.One)));
        await pair.Client.WaitAssertion(() =>
        {
            var clientGrid = pair.Client.EntMan.GetComponent<AreaGridComponent>(map.CGridUid);
            Assert.That(clientGrid.Colors, Contains.Key(Vector2i.One));
            var window = new MCBeaconChooseWindow();
            window.OpenFor(targets, new object(), 150);
            var tacticalMap = window.FindControl<TacticalMapControl>("Map");
            var list = window.FindControl<ItemList>("BeaconList");
            var submit = window.FindControl<Button>("SubmitButton");
            Assert.That(tacticalMap.Texture, Is.Not.Null);
            Assert.That(submit.Disabled, Is.True);

            tacticalMap.OnBlipClicked!(targets[1].Tile);
            Assert.That(list[1].Selected, Is.True);
            Assert.That(submit.Disabled, Is.False);
            window.Refresh([targets[0]], 150);
            Assert.That(submit.Disabled, Is.True);
            window.OpenFor([], new object(), 0);
            Assert.That(window.IsOpen, Is.True);
            Assert.That(list.Count, Is.Zero);
            Assert.That(submit.Disabled, Is.True);
            window.Close();
        });

        await server.WaitPost(() => server.System<SharedMapSystem>().DeleteMap(map.MapId));
        await pair.CleanReturnAsync();
    }
}
