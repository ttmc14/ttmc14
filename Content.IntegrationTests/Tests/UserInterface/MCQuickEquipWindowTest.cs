#nullable enable
using Content.Client._MC.Vending.UI;
using Content.Shared._MC.Serialization.Loadout.Data;
using Content.Shared._MC.Serialization.Loadout.Parts;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.Input;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Content.IntegrationTests.Tests.UserInterface;

[TestFixture]
public sealed class MCQuickEquipWindowTest
{
    [Test]
    public async Task PreviewBeforeVending()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings { Connected = true, NoLoadTestPrototypes = true });
        var client = pair.Client;
        MCVendorQuickEquip window = null!;
        Button vend = null!;
        Collapsible group = null!;
        var requests = 0;
        var uniform = new MCLoadoutItem { ProtoId = "MCTagilaPants" };
        uniform.Parts.Add(new MCLoadoutWebbing { Item = new MCLoadoutItem { ProtoId = "MCWebbingBlackVest" } });
        var loadout = new MCLoadout { Slots = [new MCLoadoutSlot { SlotName = "jumpsuit", Item = uniform }] };

        await client.WaitAssertion(() =>
        {
            window = new MCVendorQuickEquip();
            window.OpenCentered();
            vend = window.FindControl<Button>("VendButton");
            Assert.That(vend.Disabled, Is.True);
            window.OnVend += selected =>
            {
                Assert.That(selected, Is.SameAs(loadout));
                requests++;
            };
            window.ShowLoadout(loadout);
            window.Measure(window.MinSize);
            window.Arrange(UIBox2.FromDimensions(default, window.MinSize));
            Assert.That(vend.Disabled, Is.False);
            var row = window.FindControl<BoxContainer>("Items").GetChild(0);
            group = (Collapsible) row.GetChild(1);
            Assert.That(((CollapsibleHeading) group.Heading!).Label.Width, Is.GreaterThan(0));
            Assert.That(group.BodyVisible, Is.False);
        });

        async Task Click(Control control)
        {
            var center = new ScreenCoordinates(control.GlobalPixelPosition + control.PixelSize / 2, control.Window!.Id);
            foreach (var state in new[] { BoundKeyState.Down, BoundKeyState.Up })
                await client.DoGuiEvent(control, new GUIBoundKeyEventArgs(EngineKeyFunctions.UIClick, state, center, default, default, default));
        }

        await Click(group.Heading!);
        await client.WaitAssertion(() =>
        {
            Assert.That(group.BodyVisible, Is.True);
            Assert.That(group.Body!.GetChild(0).ChildCount, Is.EqualTo(1));
            Assert.That(requests, Is.Zero);
        });
        await Click(vend);
        await Click(vend);
        await client.WaitAssertion(() =>
        {
            Assert.That(requests, Is.EqualTo(1));
            window.ShowLoadout(new MCLoadout());
            Assert.That(vend.Disabled, Is.True);
            Assert.That(window.FindControl<BoxContainer>("Items").ChildCount, Is.Zero);
            window.Close();
            window.Dispose();
        });
        await pair.CleanReturnAsync();
    }
}
