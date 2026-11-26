using System.Linq;
using Content.Server._CE.ZLevels.Core;
using Content.Shared._MC.Areas;
using Content.Shared._MC.Armor.Modules.Core;
using Content.Shared._MC.Armor.Modules.Core.Components;
using Content.Shared._MC.Armor.Modules.Features.Antenna;
using Content.Shared._MC.ASRS.Components;
using Content.Shared._MC.ASRS.Systems;
using Content.Shared._MC.ASRS.Ui;
using Content.Shared._MC.ASRS.UI.Messages;
using Content.Shared._MC.ASRS.UI.Messages.Approve;
using Content.Shared._MC.ASRS.UI.Messages.Delivery;
using Content.Shared._MC.Engineering.Beacon;
using Content.Shared._MC.Engineering.Beacon.Components;
using Content.Shared._RMC14.Areas;
using Content.Shared._RMC14.Rules;
using Content.Shared.Actions;
using Content.Shared.Actions.Components;
using Content.Shared.Inventory;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class MCHelmetAntennaTest
{
    [Test]
    public async Task WornAntennaRegistersFixedTileAndSwitchesOffOnMapChange()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Map = "MCTalos", NoLoadTestPrototypes = true });
        var server = pair.Server;
        var entities = server.EntMan;
        var tileDefinitions = server.ResolveDependency<ITileDefinitionManager>();
        EntityUid first = default, second = default;
        MapId firstId = default, secondId = default;
        EntityUid user = default, module = default, helmet = default, console = default, signal = default;
        NetEntity originalSignal = default;
        MCModuleAntennaComponent antenna = null!;
        EntityUid network = default;

        void Toggle()
        {
            var action = antenna.ActionEntity!.Value;
            var component = entities.GetComponent<ActionComponent>(action);
            Assert.That(component.AttachedEntity, Is.EqualTo(user));
            server.System<SharedActionsSystem>().PerformAction(user, (action, component));
        }

        MCBeaconSystem.NetBeaconWithName[] Destinations(NetEntity id)
        {
            Assert.That(server.System<SharedUserInterfaceSystem>().TryGetUiState<MCASRSConsoleBuiState>(
                console, MCASRSConsoleUI.Key, out var state), Is.True);
            return state!.Beacons.Where(entry => entry.Uid == id).ToArray();
        }

        void CheckDelivery(NetEntity target, bool available)
        {
            var orders = entities.GetComponent<MCASRSConsoleComponent>(console);
            var index = orders.CachedEntries.FindIndex(entry => entry.Entities.Count == 1 && entry.Entities[0] == "MCBeaconSupply");
            entities.EventBus.RaiseLocalEvent(console, new MCASRSConsoleStoreRequestsMessage("Resupply", new() { [index] = 1 }) { Actor = user });
            var request = orders.Requests.Single();
            var cargo = server.System<MCASRSSystem>();
            cargo.SetBalance(request.TotalCost + orders.DeliveryFee);
            entities.EventBus.RaiseLocalEvent(console, new MCASRSConsoleApproveMessage(request.Id) { Actor = user });
            entities.EventBus.RaiseLocalEvent(console, new MCASRSConsoleDeliveryMessage(request.Id, target) { Actor = user });
            Assert.That(orders.RequestsAwaitingDelivery.Contains(request), Is.EqualTo(!available));
            Assert.That(cargo.GetBalance(), Is.EqualTo(available ? 0 : orders.DeliveryFee));
        }

        await server.WaitAssertion(() =>
        {
            var maps = server.System<SharedMapSystem>();
            first = maps.CreateMap(out firstId);
            second = maps.CreateMap(out secondId);
            var tile = new Tile(tileDefinitions["CMFloorPlating"].TileId);
            foreach (var floor in new[] { first, second })
            {
                var grid = entities.EnsureComponent<MapGridComponent>(floor);
                maps.SetTile(floor, grid, Vector2i.Zero, tile);
                maps.SetTile(floor, grid, new Vector2i(1, 0), tile);
                maps.SetTile(floor, grid, new Vector2i(0, 1), tile);
            }

            var levels = server.System<CEZLevelsSystem>();
            var zNetwork = levels.CreateZNetwork();
            network = zNetwork.Owner;
            Assert.That(levels.TryAddMapsIntoZNetwork(zNetwork, new() { [first] = 0, [second] = 1 }), Is.True);
            entities.EnsureComponent<RMCPlanetComponent>(first);
            var coords = new EntityCoordinates(first, 0.5f, 0.5f);
            user = entities.SpawnEntity("MobHuman", coords);
            console = entities.SpawnEntity("MCASRSConsole", new EntityCoordinates(first, 0.5f, 1.5f));
            module = entities.SpawnEntity("MCArmorModuleHelmetAntenna", coords);
            antenna = entities.GetComponent<MCModuleAntennaComponent>(module);
            helmet = entities.SpawnEntity("MCArmorHelmetModularXenonautenM10X", coords);
            Assert.That(server.System<InventorySystem>().TryEquip(user, helmet, "head", force: true), Is.True);
            Assert.That(server.System<MCArmorModuleSharedSystem>().TryAttachModuleToAnySlot(
                (helmet, entities.GetComponent<MCArmorModularClothingComponent>(helmet)),
                (module, entities.GetComponent<MCArmorModuleComponent>(module))), Is.True);
            Toggle();
            signal = antenna.Signal!.Value;
            originalSignal = entities.GetNetEntity(signal);
            var destination = Destinations(originalSignal).Single();
            Assert.That(destination.Name, Is.EqualTo($"{entities.GetComponent<MetaDataComponent>(user).EntityName} + {server.System<MCAreasSystem>().GetAreaName(user)}"));
            Assert.That(destination.Grid, Is.EqualTo(entities.GetNetEntity(first)));
            Assert.That(destination.Tile, Is.EqualTo(Vector2i.Zero));
            server.System<SharedTransformSystem>().SetCoordinates(user, new EntityCoordinates(first, 1.5f, 0.5f));
        });
        await pair.RunTicksSync(2);
        await server.WaitAssertion(() =>
        {
            Assert.That(entities.GetComponent<TransformComponent>(signal).Coordinates.Position.X, Is.EqualTo(0.5f));
            CheckDelivery(entities.GetNetEntity(signal), true);
            Assert.That(entities.EntityQuery<MCASRSDroppedComponent>().Single().TargetCoordinates,
                Is.EqualTo(new EntityCoordinates(first, 0.5f, 0.5f)));
            Assert.That(server.System<CEZLevelsSystem>().TryMoveUp(user), Is.True);
            Assert.That(antenna.Signal, Is.Null);
            Assert.That(Destinations(originalSignal), Is.Empty);
            CheckDelivery(originalSignal, false);
        });
        await pair.RunTicksSync(2);
        await server.WaitAssertion(() =>
        {
            Assert.That(antenna.Signal, Is.Null);
            Assert.That(entities.GetComponent<ActionComponent>(antenna.ActionEntity!.Value).Toggled, Is.False);
            Assert.That(entities.Deleted(signal), Is.True);
            Assert.That(Destinations(originalSignal), Is.Empty);
            Toggle();
            Assert.That(entities.GetComponent<TransformComponent>(user).GridUid, Is.EqualTo(second));
            Assert.That(antenna.Signal, Is.Not.Null, "The wearer must be able to activate the antenna on the destination floor.");
            var newSignal = entities.GetNetEntity(antenna.Signal!.Value);
            var destination = Destinations(newSignal).Single();
            Assert.That(destination.Grid, Is.EqualTo(entities.GetNetEntity(second)));
            Assert.That(destination.Tile, Is.EqualTo(new Vector2i(1, 0)));
            CheckDelivery(newSignal, true);
            Assert.That(entities.EntityQuery<MCASRSDroppedComponent>().Select(drop => drop.TargetCoordinates),
                Does.Contain(new EntityCoordinates(second, 1.5f, 0.5f)));
            Toggle();
            Assert.That(Destinations(newSignal), Is.Empty);
            server.System<SharedTransformSystem>().SetCoordinates(user, new EntityCoordinates(second, 0.5f, 0.5f));
            var areaGrid = entities.EnsureComponent<AreaGridComponent>(second);
            var areas = server.System<AreaSystem>();
            areas.ReplaceArea(areaGrid, Vector2i.Zero, "MCAreaIceCavesNorthCaves");
            Toggle();
            Assert.That(antenna.Signal, Is.Null, "A deep cave must block activation.");
            areas.ReplaceArea(areaGrid, Vector2i.Zero, "MCAreaIceCavesEastCaves");
            entities.EnsureComponent<MCBlockAntennaSignalComponent>(second);
            Toggle();
            Assert.That(antenna.Signal, Is.Null, "A dropship grid must block activation.");
            entities.RemoveComponent<MCBlockAntennaSignalComponent>(second);
            Toggle();
            signal = antenna.Signal!.Value;
            var registered = entities.GetNetEntity(signal);
            var inventory = server.System<InventorySystem>();
            Assert.That(inventory.TryUnequip(user, "head", force: true), Is.True);
            Assert.That(entities.HasComponent<MCBeaconComponent>(signal), Is.False);
            Assert.That(Destinations(registered), Is.Empty);
            Assert.That(antenna.ActionEntity, Is.Null);
            CheckDelivery(registered, false);
            user = entities.SpawnEntity("MobHuman", new EntityCoordinates(second, 0.5f, 0.5f));
            Assert.That(inventory.TryEquip(user, helmet, "head", force: true), Is.True);
            Assert.That(antenna.Signal, Is.Null);
            Toggle();
            signal = antenna.Signal!.Value;
            registered = entities.GetNetEntity(signal);
            server.System<MCArmorModuleSharedSystem>().DetachSpecificModule(
                (helmet, entities.GetComponent<MCArmorModularClothingComponent>(helmet)),
                (module, entities.GetComponent<MCArmorModuleComponent>(module)), user);
            Assert.That(entities.HasComponent<MCBeaconComponent>(signal), Is.False);
            Assert.That(Destinations(registered), Is.Empty);
            Assert.That(antenna.ActionEntity, Is.Null);

            foreach (var deleteHelmet in new[] { false, true })
            {
                module = entities.SpawnEntity("MCArmorModuleHelmetAntenna", new EntityCoordinates(second, 0.5f, 0.5f));
                antenna = entities.GetComponent<MCModuleAntennaComponent>(module);
                Assert.That(server.System<MCArmorModuleSharedSystem>().TryAttachModuleToAnySlot(
                    (helmet, entities.GetComponent<MCArmorModularClothingComponent>(helmet)),
                    (module, entities.GetComponent<MCArmorModuleComponent>(module))), Is.True);
                Toggle();
                signal = antenna.Signal!.Value;
                registered = entities.GetNetEntity(signal);
                entities.DeleteEntity(deleteHelmet ? helmet : module);
                Assert.That(entities.HasComponent<MCBeaconComponent>(signal), Is.False);
                Assert.That(Destinations(registered), Is.Empty);
            }
        });
        await server.WaitPost(() =>
        {
            server.System<SharedMapSystem>().DeleteMap(firstId);
            server.System<SharedMapSystem>().DeleteMap(secondId);
            entities.DeleteEntity(network);
        });
        await pair.CleanReturnAsync();
    }
}
