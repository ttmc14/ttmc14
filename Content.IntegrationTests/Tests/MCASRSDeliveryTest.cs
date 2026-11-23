using System.Collections.Generic;
using Content.Shared._MC._Config;
using Content.Shared._MC.ASRS;
using Content.Shared._MC.ASRS.Components;
using Content.Shared._MC.ASRS.Systems;
using Content.Shared._MC.ASRS.Ui;
using Content.Shared._MC.ASRS.UI.Messages;
using Content.Shared._MC.ASRS.UI.Messages.Approve;
using Content.Shared._MC.ASRS.UI.Messages.Delivery;
using Content.Shared._MC.ASRS.UI.Messages.Deny;
using Content.Shared._MC.Engineering.Miners.Components;
using Content.Shared._RMC14.Marines;
using Content.Shared.Interaction;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Prototypes;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class MCASRSDeliveryTest
{
    private const string SidePrototypes = """
        - type: MCASRSAccount
          id: TestASRSSecondSide
          startingBalance: 149
        - type: entity
          parent: MCASRSConsole
          id: TestASRSSecondConsole
          components:
          - type: MCASRSConsole
            account: TestASRSSecondSide
        - type: entity
          parent: MCASRSExportPad
          id: TestASRSSecondPad
          components:
          - type: MCASRSExporter
            account: TestASRSSecondSide
        - type: entity
          parent: MCMiner
          id: TestASRSSecondMiner
          components:
          - type: MCMiner
            account: TestASRSSecondSide
        """;

    [Test]
    public void OrderValidationChecksQuantitiesCrateCapacityAndCost()
    {
        var bundle = new MCASRSEntry { Cost = 25, Entities = ["Wrench", "Crowbar"] };
        var costly = new MCASRSEntry { Cost = int.MaxValue, Entities = ["Wrench"] };
        MCASRSRequest? Order(Dictionary<int, int>? quantities) =>
            MCASRSRequest.Create("Buyer", "Tools", quantities, [bundle, costly], 4);

        Assert.That(Order(null), Is.Null);
        Assert.That(Order(new()), Is.Null);
        Assert.That(Order(new() { [0] = 0 }), Is.Null);
        Assert.That(Order(new() { [0] = -1 }), Is.Null);
        Assert.That(Order(new() { [2] = 1 }), Is.Null);
        Assert.That(Order(new() { [0] = 3 }), Is.Null);
        Assert.That(Order(new() { [1] = 2 }), Is.Null);
        Assert.That(Order(new() { [1] = 1, [0] = 1 }), Is.Null);
        Assert.That(Order(new() { [1] = 1 })!.TotalCost, Is.EqualTo(int.MaxValue));
        Assert.That(Order(new() { [0] = 2 })!.TotalCost, Is.EqualTo(50));
    }

    [Test]
    public async Task OffPlanetDeliveryFollowsServerSetting()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Map = "MCTalos", NoLoadTestPrototypes = true });
        await pair.LoadPrototypes(["""
            - type: entity
              parent: MCBeaconSupply
              id: TestShipboardSupplyBeacon
              components:
              - type: MCDeploy
                state: Deployed
            """]);
        var server = pair.Server;
        var entities = server.EntMan;
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        await server.WaitAssertion(() =>
        {
            var coords = new EntityCoordinates(map.Grid, 0.5f, 0.5f);
            server.System<SharedMapSystem>().SetTile(map.Grid, new Vector2i(1, 0), map.Tile.Tile);
            var console = entities.SpawnEntity("MCASRSConsole", new EntityCoordinates(map.Grid, 1.5f, 0.5f));
            var user = entities.SpawnEntity("MobHuman", coords);
            var folded = entities.GetNetEntity(entities.SpawnEntity("MCBeaconSupply", coords));
            var deployed = entities.GetNetEntity(entities.SpawnEntity("TestShipboardSupplyBeacon", coords));
            var canterbury = entities.GetNetEntity(entities.SpawnEntity("MCMarkerTgsCanterbury", coords));
            var orders = entities.GetComponent<MCASRSConsoleComponent>(console);
            var index = orders.CachedEntries.FindIndex(entry => entry.Entities.Count == 1 && entry.Entities[0] == "MCBeaconSupply");
            entities.EventBus.RaiseLocalEvent(console,
                new MCASRSConsoleStoreRequestsMessage("Ship delivery", new() { [index] = 1 }) { Actor = user });
            var request = orders.Requests[0];
            var cargo = server.System<MCASRSSystem>();
            cargo.SetBalance(request.TotalCost + orders.DeliveryFee);
            entities.EventBus.RaiseLocalEvent(console, new MCASRSConsoleApproveMessage(request.Id) { Actor = user });
            var ui = server.System<SharedUserInterfaceSystem>();
            Assert.That(ui.TryGetUiState<MCASRSConsoleBuiState>(console, MCASRSConsoleUI.Key, out var state), Is.True);
            Assert.That(state!.Beacons.Exists(entry => entry.Uid == folded), Is.False);
            Assert.That(state.Beacons.Exists(entry => entry.Uid == deployed), Is.False);
            Assert.That(state.Beacons.Exists(entry => entry.Uid == canterbury), Is.True);

            void SetDeliveryAnywhere(bool allow)
            {
                server.CfgMan.SetCVar(MCConfigVars.AsrsAllowDeliveryAnywhere, allow);
                Assert.That(ui.TryGetUiState<MCASRSConsoleBuiState>(console, MCASRSConsoleUI.Key, out var updated), Is.True);
                Assert.That(updated!.Beacons.Exists(entry => entry.Uid == deployed), Is.EqualTo(allow));
                Assert.That(updated.Beacons.Exists(entry => entry.Uid == folded), Is.False);
                Assert.That(updated.Beacons.Exists(entry => entry.Uid == canterbury), Is.True);
            }

            SetDeliveryAnywhere(true);
            SetDeliveryAnywhere(false);
            entities.EventBus.RaiseLocalEvent(console, new MCASRSConsoleDeliveryMessage(request.Id, deployed) { Actor = user });
            Assert.That(orders.RequestsAwaitingDelivery, Does.Contain(request));
            Assert.That(cargo.GetBalance(), Is.EqualTo(orders.DeliveryFee));

            SetDeliveryAnywhere(true);
            entities.EventBus.RaiseLocalEvent(console, new MCASRSConsoleDeliveryMessage(request.Id, deployed) { Actor = user });
            Assert.That(orders.RequestsAwaitingDelivery, Is.Empty);
            Assert.That(cargo.GetBalance(), Is.Zero);
            SetDeliveryAnywhere(false);
        });
        await server.WaitPost(() => server.System<SharedMapSystem>().DeleteMap(map.MapId));
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task ConsolesChargeTheirOwnAccountAndDeliveryFee()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Map = "MCTalos",
            NoLoadTestPrototypes = true,
        });
        await pair.LoadPrototypes([SidePrototypes]);
        var server = pair.Server;
        var entities = server.EntMan;
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        await server.WaitAssertion(() =>
        {
            var cargo = server.System<MCASRSSystem>();
            var ui = server.System<SharedUserInterfaceSystem>();
            var coords = new EntityCoordinates(map.Grid, 0.5f, 0.5f);
            server.System<SharedMapSystem>().SetTile(map.Grid, new Vector2i(1, 0), map.Tile.Tile);
            var consoleCoords = new EntityCoordinates(map.Grid, 1.5f, 0.5f);
            var console = entities.SpawnEntity("TestASRSSecondConsole", consoleCoords);
            var peer = entities.SpawnEntity("TestASRSSecondConsole", consoleCoords);
            var crash = entities.SpawnEntity("MCASRSConsoleCrash", consoleCoords);
            ProtoId<MCASRSAccountPrototype> side = "TestASRSSecondSide";
            var user = entities.SpawnEntity("MobHuman", coords);
            var beacon = entities.GetNetEntity(entities.SpawnEntity("MCMarkerTgsCanterbury", coords));
            MCASRSRequest Submit(EntityUid target)
            {
                var comp = entities.GetComponent<MCASRSConsoleComponent>(target);
                var index = comp.CachedEntries.FindIndex(entry => entry.Entities.Count == 1 && entry.Entities[0] == "MCBeaconSupply");
                Assert.That(index, Is.GreaterThanOrEqualTo(0));
                entities.EventBus.RaiseLocalEvent(target,
                    new MCASRSConsoleStoreRequestsMessage("Tools", new() { [index] = 1 }) { Actor = user });
                return comp.Requests[^1];
            }

            var component = entities.GetComponent<MCASRSConsoleComponent>(console);
            var pending = component.RequestsAwaitingDelivery;
            var first = Submit(console);
            var second = Submit(console);
            Assert.That(first.Id, Is.Not.EqualTo(second.Id));
            Assert.That(cargo.GetBalance(side), Is.EqualTo(149));
            Assert.That(cargo.GetBalance(), Is.EqualTo(server.CfgMan.GetCVar(MCConfigVars.AsrsStartingBalance)));

            cargo.SetBalance(first.TotalCost, side);
            entities.EventBus.RaiseLocalEvent(console, new MCASRSConsoleApproveAllMessage { Actor = user });
            Assert.That(pending, Is.Empty);
            Assert.That(cargo.GetBalance(side), Is.EqualTo(first.TotalCost));

            void ApproveFirst() => entities.EventBus.RaiseLocalEvent(console,
                new MCASRSConsoleApproveMessage(first.Id) { Actor = user });
            cargo.SetBalance(first.TotalCost + second.TotalCost, side);
            ApproveFirst();
            ApproveFirst();
            entities.EventBus.RaiseLocalEvent(console, new MCASRSConsoleDenyMessage(first.Id) { Actor = user });
            Assert.That(component.RequestsDenyHistory, Is.Empty);
            Assert.That(component.Requests, Is.EqualTo(new[] { second }));
            Assert.That(cargo.GetBalance(side), Is.EqualTo(second.TotalCost));
            entities.EventBus.RaiseLocalEvent(console, new MCASRSConsoleApproveAllMessage { Actor = user });
            Assert.That(pending, Is.EqualTo(new[] { first, second }));
            Assert.That(cargo.GetBalance(side), Is.Zero);

            void Send() => entities.EventBus.RaiseLocalEvent(console,
                new MCASRSConsoleDeliveryMessage(first.Id, beacon) { Actor = user });

            cargo.SetBalance(149, side);
            cargo.SetBalance(600);
            cargo.AddBalance(1, side);
            Assert.That(ui.TryGetUiState<MCASRSConsoleBuiState>(console, MCASRSConsoleUI.Key, out var state), Is.True);
            Assert.That(state!.DeliveryFee, Is.EqualTo(150));
            Assert.That(state.Beacons.Exists(entry => entry.Uid == beacon), Is.True);
            entities.EnsureComponent<AlmayerComponent>(map.Grid);
            cargo.SetBalance(149, side);
            Assert.That(ui.TryGetUiState<MCASRSConsoleBuiState>(console, MCASRSConsoleUI.Key, out state), Is.True);
            Assert.That(state!.Beacons.Exists(entry => entry.Uid == beacon), Is.True);
            Send();
            Assert.That(pending, Has.Count.EqualTo(2));
            Assert.That(cargo.GetBalance(side), Is.EqualTo(149));

            cargo.SetBalance(150, side);
            var wall = entities.SpawnEntity("MCWallInvincibleNoIcon", coords);
            Send();
            Assert.That(pending, Has.Count.EqualTo(2));
            Assert.That(cargo.GetBalance(side), Is.EqualTo(150));
            entities.DeleteEntity(wall);
            Send();
            Assert.That(pending, Is.EqualTo(new[] { second }));
            Assert.That(cargo.GetBalance(side), Is.Zero);
            Assert.That(cargo.GetBalance(), Is.EqualTo(600));
            Assert.That(ui.TryGetUiState<MCASRSConsoleBuiState>(peer, MCASRSConsoleUI.Key, out state), Is.True);
            Assert.That(state!.Points, Is.Zero);

            cargo.SetBalance(150, side);
            Send();
            Assert.That(cargo.GetBalance(side), Is.EqualTo(150));

            Assert.That(pending, Is.EqualTo(new[] { second }));
            entities.EventBus.RaiseLocalEvent(console, new MCASRSConsoleDeliveryMessage(second.Id, beacon) { Actor = user });
            Assert.That(pending, Is.Empty);
            Assert.That(cargo.GetBalance(side), Is.Zero);

            var crashOrder = Submit(crash);
            cargo.SetBalance(crashOrder.TotalCost);
            entities.EventBus.RaiseLocalEvent(crash, new MCASRSConsoleApproveMessage(crashOrder.Id) { Actor = user });
            Assert.That(cargo.GetBalance(), Is.Zero);
            entities.EventBus.RaiseLocalEvent(crash, new MCASRSConsoleDeliveryMessage(crashOrder.Id, beacon) { Actor = user });
            Assert.That(entities.GetComponent<MCASRSConsoleComponent>(crash).RequestsAwaitingDelivery, Is.Empty);
            Assert.That(cargo.GetBalance(), Is.Zero);

            var miner = entities.SpawnEntity("TestASRSSecondMiner", coords);
            var drill = entities.GetComponent<MCMinerComponent>(miner);
            drill.State = MCMinerState.Running;
            drill.MineralStored = 2;
            entities.EventBus.RaiseLocalEvent(miner, new InteractHandEvent(user, miner));
            Assert.That(cargo.GetBalance(side), Is.EqualTo(2 * drill.MineralValueTotal));
            Assert.That(cargo.GetBalance(), Is.Zero);

            var pad = entities.SpawnEntity("TestASRSSecondPad", coords);
            var item = entities.SpawnEntity("Wrench", new EntityCoordinates(map.Grid, 0.5f, 0.35f));
            entities.EnsureComponent<MCASRSCostComponent>(item).Cost = 37;
            var exporter = entities.GetComponent<MCASRSExporterComponent>(pad);
            server.System<MCASRSExporterSystem>().Sell((pad, exporter));
            Assert.That(cargo.GetBalance(side), Is.EqualTo(2 * drill.MineralValueTotal + 37));
            Assert.That(cargo.GetBalance(), Is.Zero);
        });
        await server.WaitPost(() => server.System<SharedMapSystem>().DeleteMap(map.MapId));
        await pair.CleanReturnAsync();
    }
}
