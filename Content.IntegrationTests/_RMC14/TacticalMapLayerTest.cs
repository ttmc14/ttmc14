using System.Linq;
using Content.Client._RMC14.TacticalMap;
using Content.Server._CE.ZLevels.Core;
using Content.Shared._RMC14.Areas;
using Content.Shared._RMC14.TacticalMap;
using Robust.Client.UserInterface;
using Robust.Shared.GameObjects;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Maths;

namespace Content.IntegrationTests._RMC14;

[TestFixture]
public sealed class TacticalMapLayerTest
{
    [Test]
    public async Task PersonalMapOpensOnPlayerLayerAndPublishesOnlyToSelectedLayer()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Connected = true,
            NoLoadTestPrototypes = true,
            Map = "MCTalos",
        });
        var server = pair.Server;
        var client = pair.Client;
        var entities = server.EntMan;
        var uiManager = client.ResolveDependency<IUserInterfaceManager>();
        var session = server.PlayerMan.GetSessionById(client.Session!.UserId);
        var previous = session.AttachedEntity;
        var tileDefinitions = server.ResolveDependency<ITileDefinitionManager>();
        var mapSystem = server.System<SharedMapSystem>();
        var key = TacticalMapUserUi.Key;
        EntityUid surface = default, basement = default, user = default, network = default;
        NetEntity surfaceNet = default, basementNet = default, userNet = default;
        MapId surfaceId = default, basementId = default;

        await server.WaitAssertion(() =>
        {
            surface = mapSystem.CreateMap(out surfaceId);
            basement = mapSystem.CreateMap(out basementId);
            foreach (var floor in new[] { surface, basement })
            {
                var grid = entities.EnsureComponent<MapGridComponent>(floor);
                mapSystem.SetTile(floor, grid, Vector2i.Zero, new Tile(tileDefinitions["CMFloorPlating"].TileId));
            }

            entities.EnsureComponent<AreaGridComponent>(surface);
            entities.EnsureComponent<TacticalMapComponent>(surface);
            var levels = server.System<CEZLevelsSystem>();
            var zNetwork = levels.CreateZNetwork();
            network = zNetwork.Owner;
            Assert.That(levels.TryAddMapsIntoZNetwork(zNetwork, new() { [surface] = 0, [basement] = -1 }), Is.True);
            Assert.That(entities.HasComponent<AreaGridComponent>(basement), Is.True);
            Assert.That(entities.HasComponent<TacticalMapComponent>(basement), Is.True);
            user = entities.SpawnEntity("CMXenoQueen", new EntityCoordinates(basement, 0.5f, 0.5f));
            server.PlayerMan.SetAttachedEntity(session, user);
            userNet = entities.GetNetEntity(user);
            surfaceNet = entities.GetNetEntity(surface);
            basementNet = entities.GetNetEntity(basement);
        });

        await pair.RunTicksSync(5);
        await server.WaitAssertion(() => server.System<SharedUserInterfaceSystem>().OpenUi(user, key, user));
        await pair.RunTicksSync(10);
        async Task Click(Control control)
        {
            var center = new ScreenCoordinates(control.GlobalPixelPosition + control.PixelSize / 2, control.Window!.Id);
            foreach (var state in new[] { BoundKeyState.Down, BoundKeyState.Up })
            {
                await client.DoGuiEvent(control, new GUIBoundKeyEventArgs(EngineKeyFunctions.UIClick, state, center, default, default, default));
            }
        }

        TacticalMapWrapper wrapper = null!;
        await client.WaitAssertion(() =>
        {
            var window = uiManager.WindowRoot.Children.OfType<TacticalMapWindow>().Single();
            wrapper = window.Wrapper;
            Assert.That(wrapper.LayerControls.Visible, Is.True);
            Assert.That(wrapper.LayerDown.Disabled, Is.True);
            Assert.That(wrapper.LayerUp.Disabled, Is.False);
        });
        await Click(wrapper.LayerUp);
        await pair.RunTicksSync(5);
        await client.WaitAssertion(() =>
        {
            Assert.That(wrapper.LayerUp.Disabled, Is.True);
            Assert.That(wrapper.LayerDown.Disabled, Is.False);
            var localUser = client.EntMan.GetEntity(userNet);
            Assert.That(client.System<SharedUserInterfaceSystem>().TryGetOpenUi<TacticalMapUserBui>(localUser, key, out var bui), Is.True);
            bui!.SendMessage(new TacticalMapUpdateCanvasMsg(basementNet, [], new() { [Vector2i.Zero] = "stale" }));
            bui.SendMessage(new TacticalMapUpdateCanvasMsg(surfaceNet, [], new() { [Vector2i.Zero] = "surface" }));
        });

        await pair.RunTicksSync(5);
        await server.WaitAssertion(() =>
        {
            Assert.That(entities.GetComponent<TacticalMapComponent>(surface).XenoLabels[Vector2i.Zero], Is.EqualTo("surface"));
            Assert.That(entities.GetComponent<TacticalMapComponent>(basement).XenoLabels, Is.Empty);
            server.PlayerMan.SetAttachedEntity(session, previous);
            mapSystem.DeleteMap(basementId);
            mapSystem.DeleteMap(surfaceId);
            entities.DeleteEntity(network);
        });
        await pair.CleanReturnAsync();
    }
}
