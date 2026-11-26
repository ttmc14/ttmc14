using System.Linq;
using Content.Client._MC.Nuke.Bomb.UI;
using Content.Server.Power.Components;
using Content.Shared._MC.Nuke.Bomb.Components;
using Content.Shared._MC.Nuke.Bomb.UI;
using Content.Shared._MC.Nuke.Generator.Components;
using Content.Shared._MC.Nuke.Generator.UI;
using Content.Shared._MC.Rules.EndRound;
using Content.Shared._MC.Shuttle.Events;
using Content.Shared._RMC14.Dropship;
using Content.Shared._RMC14.Rules;
using Content.Shared.Alert;
using Content.Shared.Containers.ItemSlots;
using Content.Shared.GameTicking.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Power;
using Content.Shared.UserInterface;
using Content.Shared.Weapons.Melee;
using Robust.Client.UserInterface;
using Robust.Client.UserInterface.Controls;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class MCNukeTest
{
    [Test]
    public async Task WarheadRequiresThreeDisksAndCanBeDisarmedBeforeDetonation()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Map = "MCTalos",
            Connected = true,
            NoLoadTestPrototypes = true,
        });
        var server = pair.Server;
        var entities = server.EntMan;
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        var coords = new EntityCoordinates(map.Grid, 0.5f, 0.5f);
        EntityUid rule = default;
        EntityUid bomb = default;
        EntityUid marine = default;
        EntityUid xeno = default;
        await server.WaitAssertion(() =>
        {
            entities.EnsureComponent<RMCPlanetComponent>(map.MapUid);
            marine = entities.SpawnEntity("MobHuman", coords);
            var session = server.PlayerMan.GetSessionById(pair.Client.Session!.UserId);
            server.PlayerMan.SetAttachedEntity(session, marine);
            bomb = entities.SpawnEntity("MCNuclearBombFilled", coords);
            var device = entities.GetComponent<MCNukeComponent>(bomb);
            device.DeployTime = TimeSpan.Zero;
            device.DisarmTime = TimeSpan.Zero;
            server.System<ActivatableUISystem>().InteractUI(marine, bomb);
            Assert.That(device.Deployed, Is.True);
        });
        await pair.RunTicksSync(5);
        var ui = pair.Client.ResolveDependency<IUserInterfaceManager>();
        await pair.Client.WaitAssertion(() =>
        {
            var window = ui.WindowRoot.Children.OfType<MCNukeWindow>().Single();
            Assert.That(window.TimerButton.Disabled, Is.True);
            Assert.That(window.FindControl<GridContainer>("Disks").ChildCount, Is.EqualTo(6));
            Assert.That(window.FindControl<Label>("TimeLeftLabel").Text, Is.EqualTo("06:00"));
        });
        await server.WaitAssertion(() =>
        {
            xeno = entities.SpawnEntity("MCXenoRunner", new EntityCoordinates(map.Grid, -0.4f, 0.5f));
            var device = entities.GetComponent<MCNukeComponent>(bomb);
            void Send<T>(T message) where T : BoundUserInterfaceMessage
            {
                message.Actor = marine;
                entities.EventBus.RaiseLocalEvent(bomb, message);
            }

            Send(new MCNukeActiveBuiMessage());
            Assert.That(device.DetonatesAt, Is.Null);
            Send(new MCNukeDiskBuiMessage("disk_blue"));
            Assert.That(server.System<ItemSlotsSystem>().GetItemOrNull(bomb, "disk_blue"), Is.Null);
            Send(new MCNukeSafetyBuiMessage());
            Assert.That(device.Safety, Is.True);
            Send(new MCNukeDiskBuiMessage("disk_blue"));
            Assert.That(server.System<ItemSlotsSystem>().GetItemOrNull(bomb, "disk_blue"), Is.Not.Null);
            Send(new MCNukeTimeBuiMessage(10));
            Assert.That(device.Time.TotalSeconds, Is.EqualTo(370));
            Send(new MCNukeSafetyBuiMessage());
            Send(new MCNukeActiveBuiMessage());
            Assert.That(device.DetonatesAt, Is.Not.Null);
            Assert.That(device.Exploded, Is.False);
            Assert.That(server.System<AlertsSystem>().IsShowingAlert(xeno, "MCNukeCountdown"), Is.True);
            Send(new MCNukeSafetyBuiMessage());
            Assert.That(device.DetonatesAt, Is.Null);
            Send(new MCNukeSafetyBuiMessage());
            Send(new MCNukeActiveBuiMessage());
            Send(new MCNukeAnchorBuiMessage());
            Assert.That(device.DetonatesAt, Is.Null);
            Send(new MCNukeAnchorBuiMessage());
            Send(new MCNukeActiveBuiMessage());
            Send(new MCNukeTimeBuiMessage(-10));
            Assert.That(device.Time.TotalSeconds, Is.EqualTo(370));
            Send(new MCNukeDiskBuiMessage("disk_blue"));
            Assert.That(device.DetonatesAt, Is.Not.Null);

            var melee = server.System<SharedMeleeWeaponSystem>();
            Assert.That(melee.TryGetWeapon(xeno, out var weaponUid, out var weapon), Is.True);
            melee.AttemptLightAttack(xeno, weaponUid, weapon!, bomb, requireCombatMode: false);
            Assert.That(device.DetonatesAt, Is.Null);
            Send(new MCNukeDiskBuiMessage("disk_blue"));
            rule = entities.SpawnEntity(null, coords);
            entities.EnsureComponent<GameRuleComponent>(rule);
            entities.EnsureComponent<ActiveGameRuleComponent>(rule);
            entities.EnsureComponent<MCRuleEndRoundComponent>(rule);
            entities.EnsureComponent<MCRuleEndRoundNukeExplodedComponent>(rule).WithoutEvacuationId = "NukeMinorVictory";
            entities.EnsureComponent<MCRuleEndRoundShuttleEvacuationComponent>(rule);
            device.Time = TimeSpan.FromMilliseconds(200);
            Send(new MCNukeActiveBuiMessage());
            entities.EventBus.RaiseEvent(EventSource.Local, new MCShuttleEvacuationEvent());
            Assert.That(entities.GetComponent<MCRuleEndRoundComponent>(rule).Result, Is.Null.Or.Empty);
            Assert.That(device.Exploded, Is.False);
        });
        await pair.RunTicksSync(30);
        await server.WaitAssertion(() =>
        {
            var device = entities.GetComponent<MCNukeComponent>(bomb);
            Assert.That(entities.GetComponent<MCRuleEndRoundComponent>(rule).Result, Is.EqualTo("NukeVictory"));
            Assert.That(device.Exploded, Is.True);
            Assert.That(device.DetonatesAt, Is.Null);
            Assert.That(server.System<MobStateSystem>().IsDead(marine), Is.True);
            entities.EventBus.RaiseLocalEvent(bomb, new MCNukeActiveBuiMessage { Actor = marine });
            Assert.That(device.DetonatesAt, Is.Null);
        });
        await server.WaitPost(() => server.System<SharedMapSystem>().DeleteMap(map.MapId));
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task GeneratorKeepsCompletedStagesAndCancelsPrintingOnPowerLossOrHijack()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Map = "MCTalos",
            Connected = true,
            NoLoadTestPrototypes = true,
        });
        var server = pair.Server;
        var entities = server.EntMan;
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        var coords = new EntityCoordinates(map.Grid, 0.5f, 0.5f);
        EntityUid terminal = default;
        EntityUid user = default;
        void Run() => entities.EventBus.RaiseLocalEvent(terminal, new MCNukeDiskGeneratorRunBuiMessage { Actor = user });
        await server.WaitAssertion(() =>
        {
            user = entities.SpawnEntity("MobHuman", coords);
            terminal = entities.SpawnEntity("MCComputerNukeDiskGeneratorBlue", coords);
            var power = entities.GetComponent<ApcPowerReceiverComponent>(terminal);
            power.NeedsPower = false;
            power.Powered = true;
            var generator = entities.GetComponent<MCNukeDiskGeneratorComponent>(terminal);
            generator.InteractionTime = TimeSpan.Zero;
            generator.StepDuration = TimeSpan.FromMilliseconds(100);
            Run();
            Assert.That(generator.StepEndsAt, Is.Not.Null);
            Run();
            var xeno = entities.SpawnEntity("MCXenoRunner", new EntityCoordinates(map.Grid, -0.4f, 0.5f));
            generator.InterruptTime = TimeSpan.Zero;
            var melee = server.System<SharedMeleeWeaponSystem>();
            Assert.That(melee.TryGetWeapon(xeno, out var weaponUid, out var weapon), Is.True);
            melee.AttemptLightAttack(xeno, weaponUid, weapon!, terminal, requireCombatMode: false);
            Assert.That(generator.StepEndsAt, Is.Null);
            entities.DeleteEntity(xeno);
            Run();
            power.Powered = false;
            var loss = new PowerChangedEvent(false, 0);
            entities.EventBus.RaiseLocalEvent(terminal, ref loss);
            Run();
            Assert.That(generator.StepEndsAt, Is.Null);
            power.Powered = true;
            Assert.That(generator.CheckpointProgress.Float(), Is.Zero);
            Run();
        });

        for (var stage = 1; stage <= 5; stage++)
        {
            await pair.RunTicksSync(15);
            await server.WaitAssertion(() =>
            {
                var generator = entities.GetComponent<MCNukeDiskGeneratorComponent>(terminal);
                Assert.That(generator.StepEndsAt, Is.Null);
                Assert.That(generator.CheckpointProgress.Float(), Is.EqualTo(stage / 5f).Within(0.001f));
                if (stage < 5)
                {
                    Run();
                }
            });
        }

        await server.WaitAssertion(() =>
        {
            var generator = entities.GetComponent<MCNukeDiskGeneratorComponent>(terminal);
            Run();
            var first = generator.Operation;
            Run();
            Assert.That(first, Is.Not.Null);
            Assert.That(generator.Operation, Is.EqualTo(first));
            var loss = new PowerChangedEvent(false, 0);
            entities.EventBus.RaiseLocalEvent(terminal, ref loss);
            Assert.That(generator.Operation, Is.Null);
            Assert.That(generator.CheckpointProgress.Float(), Is.EqualTo(1));

            generator.PrintTime = TimeSpan.Zero;
            Run();
            var disks = 0;
            var items = entities.EntityQueryEnumerator<MetaDataComponent, TransformComponent>();
            while (items.MoveNext(out var meta, out var transform))
            {
                if (transform.MapID == map.MapId && meta.EntityPrototype?.ID == "MCDiskNukeBlue")
                {
                    disks++;
                }
            }
            Assert.That(disks, Is.EqualTo(1));

            generator.PrintTime = TimeSpan.FromSeconds(15);
            Run();
            var hijack = new DropshipHijackStartEvent(null);
            entities.EventBus.RaiseEvent(EventSource.Local, ref hijack);
            Assert.That(generator.Operation, Is.Null);
            Assert.That(generator.Disabled, Is.True);
            Run();
            Assert.That(generator.Operation, Is.Null);
        });
        await server.WaitPost(() => server.System<SharedMapSystem>().DeleteMap(map.MapId));
        await pair.CleanReturnAsync();
    }
}
