using System.Linq;
using System.Numerics;
using Content.Client._MC.Vending.UI;
using Content.Client._RMC14.Vendors;
using Content.Shared._RMC14.Vendors;
using Robust.Client.GameObjects;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.GameObjects;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Maths;
using Robust.Shared.Utility;

namespace Content.IntegrationTests.Tests.UserInterface;

[TestFixture]
public sealed class MCVendorWindowTest
{
    [TestCase("MCMachineVendingQuickFood", 1)]
    [TestCase("MCMachineVendingWeapon", 14)]
    [TestCase("MCMachineVendingNanotrasenMed", 2)]
    public async Task CategoryMenuDispensesFilteredItem(string vendorPrototype, int categoryIndex)
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Connected = true,
            Map = "MCTalos",
            NoLoadTestPrototypes = true,
        });
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        var client = pair.Client;
        var server = pair.Server;
        var vendor = EntityUid.Invalid;
        var actor = EntityUid.Invalid;
        await server.WaitPost(() =>
        {
            actor = server.EntMan.SpawnEntity("MobHuman", map.GridCoords);
            vendor = server.EntMan.SpawnEntity(vendorPrototype, map.GridCoords);
            var session = server.PlayerMan.GetSessionById(client.Session!.UserId);
            server.PlayerMan.SetAttachedEntity(session, actor);
        });
        await pair.RunTicksSync(5);
        await server.WaitPost(() => server.System<SharedUserInterfaceSystem>().OpenUi(vendor, CMAutomatedVendorUI.Key, actor));
        await pair.RunTicksSync(5);

        var ui = client.ResolveDependency<IUserInterfaceManager>();
        async Task Click(BaseButton button)
        {
            var center = new ScreenCoordinates(button.GlobalPixelPosition + button.PixelSize / 2, button.Window!.Id);
            foreach (var state in new[] { BoundKeyState.Down, BoundKeyState.Up })
            {
                await client.DoGuiEvent(button, new GUIBoundKeyEventArgs(EngineKeyFunctions.UIClick, state, center, default, default, default));
            }
        }

        var window = ui.WindowRoot.Children.OfType<MCVendorWindow>().Single();
        var categories = window.Categories;
        await client.WaitAssertion(() =>
        {
            var size = new Vector2(470, 510);
            window.Measure(size);
            window.Arrange(UIBox2.FromDimensions(default, size));
            foreach (var section in window.Sections.Children.Cast<MCVendorSectionView>().Where(section => section.Visible))
            {
                var rows = section.Rows.Children.Cast<CMAutomatedVendorEntry>().ToArray();
                var widestIcon = rows.SelectMany(row => row.Texture.Textures).Select(texture => texture.Width).DefaultIfEmpty().Max();
                foreach (var row in rows)
                {
                    Assert.That(row.Texture.Width, Is.EqualTo(widestIcon));
                    Assert.That(row.Panel.GlobalPosition.X, Is.EqualTo(rows[0].Panel.GlobalPosition.X));
                    Assert.That(row.Panel.Width, Is.EqualTo(rows[0].Panel.Width));
                }
            }

            Assert.That(categories.Visible, Is.True);
            Assert.That(categories.SelectedId, Is.Zero);
            if (vendorPrototype == "MCMachineVendingWeapon")
            {
                var rifles = (MCVendorSectionView) window.Sections.GetChild(0);
                var rifle = (CMAutomatedVendorEntry) rifles.Rows.GetChild(2);
                var sprites = client.System<SpriteSystem>();
                var path = new ResPath("_MC/Objects/Weapons/Guns/Rifles/ar21/icon.rsi");
                Assert.That(rifle.Texture.Textures, Does.Contain(sprites.GetState(new SpriteSpecifier.Rsi(path, "base")).Frame0));
                Assert.That(rifle.Texture.Textures, Does.Contain(sprites.GetState(new SpriteSpecifier.Rsi(path, "mag-0")).Frame0));
                var marksman = (MCVendorSectionView) window.Sections.GetChild(4);
                var cartridges = (CMAutomatedVendorEntry) marksman.Rows.GetChild(3);
                var handful = new SpriteSpecifier.Rsi(new ResPath("_MC/Objects/Weapons/Guns/Ammunition/Handful/mosin.rsi"), "base-5");
                Assert.That(cartridges.Texture.Textures, Does.Contain(sprites.GetState(handful).Frame0));
            }
            if (vendorPrototype == "MCMachineVendingNanotrasenMed")
            {
                var equipment = (MCVendorSectionView) window.Sections.GetChild(3);
                var hud = (CMAutomatedVendorEntry) equipment.Rows.GetChild(6);
                var active = new SpriteSpecifier.Rsi(new ResPath("_MC/Clothing/Eyes/Glasses/Hud/medical.rsi"), "icon-on");
                Assert.That(hud.Texture.Textures, Does.Contain(client.System<SpriteSystem>().GetState(active).Frame0));
            }
            Assert.That(window.Sections.GetChild(0).Visible, Is.True);
            Assert.That(window.Sections.GetChild(categoryIndex).Visible, Is.True);
        });
        await Click(categories);
        var options = categories.OptionsScroll.Children.OfType<BoxContainer>().Single();
        await Click((Button) options.GetChild(categoryIndex + 1));

        CMAutomatedVendorEntry vend = null!;
        await client.WaitAssertion(() =>
        {
            var search = window.FindControl<LineEdit>("Search");
            var sections = window.Sections.Children.Cast<MCVendorSectionView>().ToArray();
            var category = sections[categoryIndex];
            Assert.That(categories.SelectedId, Is.EqualTo(categoryIndex + 1));
            Assert.That(category.Visible, Is.True);
            Assert.That(sections[0].Visible, Is.False);
            var row = (CMAutomatedVendorEntry) category.Rows.GetChild(1);
            Assert.That(row.Texture.Textures, Is.Not.Empty);
            search.SetText(row.Panel.Button.Label.Text!, invokeEvent: true);
            Assert.That(row.Visible, Is.True);
            Assert.That(category.Rows.GetChild(0).Visible, Is.False);
            vend = row;
            Assert.That(vend.Panel.Button.Disabled, Is.False);
        });

        await Click(vend.Panel.Button);

        await pair.RunTicksSync(5);
        await server.WaitAssertion(() =>
        {
            var entries = server.EntMan.GetComponent<CMAutomatedVendorComponent>(vendor).Sections[categoryIndex].Entries;
            Assert.That(server.EntMan.EntityQuery<MetaDataComponent>().Any(meta => meta.EntityPrototype?.ID == entries[1].Id.Id), Is.True);
        });
        await pair.CleanReturnAsync();
    }
}
