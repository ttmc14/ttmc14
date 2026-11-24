using System.Linq;
using System.Numerics;
using Content.Client._RMC14.Xenonids.Hud;
using Content.Shared._MC.Aura;
using Content.Shared._MC.Aura.Components;
using Content.Shared._MC.Damage;
using Content.Shared._MC.Xeno.Abilities.Drone.ResinJelly.Components;
using Content.Shared._MC.Xeno.Abilities.Drone.ResinJelly.Events;
using Content.Shared._MC.Xeno.Abilities.Gorger.Carnage.Events;
using Content.Shared._MC.Xeno.Abilities.Gorger.Drain.Components;
using Content.Shared._MC.Xeno.Abilities.Gorger.Drain.Events;
using Content.Shared._MC.Xeno.Abilities.Gorger.Feast.Events;
using Content.Shared._MC.Xeno.Abilities.Gorger.Oppose.Events;
using Content.Shared._MC.Xeno.Abilities.Gorger.PsychicLink.Components;
using Content.Shared._MC.Xeno.Abilities.Gorger.PsychicLink.Events;
using Content.Shared._MC.Xeno.Abilities.Gorger.Transfusion.Components;
using Content.Shared._MC.Xeno.Abilities.Gorger.Transfusion.Events;
using Content.Shared._MC.Xeno.Abilities.Heal.HealingInfusion.Components;
using Content.Shared._MC.Xeno.Abilities.Heal.HealingInfusion.Events;
using Content.Shared._MC.Xeno.Abilities.Ravager.Rage.Systems;
using Content.Shared._MC.Xeno.Heal;
using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._MC.Xeno.Plasma.Systems;
using Content.Shared._RMC14.Actions;
using Content.Shared.Actions;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Maths;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class MCXenoGorgerTest
{
    [TestCase(0, "fury0")]
    [TestCase(40, "fury10")]
    [TestCase(100, "fury30")]
    [TestCase(200, "fury50")]
    [TestCase(400, "fury100")]
    public void BloodHudUsesFullCapacity(int blood, string state)
    {
        Assert.That(XenoHudOverlay.GetBloodHudState(blood, 400), Is.EqualTo(state));
    }

    [Test]
    public async Task TransfusionSpendsBloodAndAbsorbsTheNextHit()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Map = "MCTalos",
            Connected = true,
            NoLoadTestPrototypes = true,
        });
        var server = pair.Server;
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        var actions = server.System<SharedActionsSystem>();
        var xenoActions = server.System<SharedRMCActionsSystem>();
        var plasma = server.System<MCXenoPlasmaSystem>();
        var health = server.System<MCXenoHealSystem>();
        EntityUid gorger = default;

        await server.WaitAssertion(() =>
        {
            gorger = server.EntMan.SpawnEntity("MCXenoGorger", map.GridCoords);
            Assert.That(server.EntMan.HasComponent<MCAuraComponent>(gorger), Is.False);
            var primordial = server.EntMan.SpawnEntity("MCXenoGorgerPrimordial", map.GridCoords);
            Assert.That(xenoActions.GetActionsWithEvent<MCXenoOpposeActionEvent>(gorger), Is.Empty);
            Assert.That(xenoActions.GetActionsWithEvent<MCXenoOpposeActionEvent>(primordial).Count(), Is.EqualTo(1));

            plasma.DrainPlasma(gorger, 25);
            Assert.That(plasma.GetPlasma(gorger), Is.EqualTo(100));
            var transfusion = xenoActions.GetActionsWithEvent<MCXenoTransfusionActionEvent>(gorger).Single();
            Assert.That(actions.SetEventTarget(transfusion, gorger), Is.True);
            actions.PerformAction(gorger, transfusion);
            Assert.That(plasma.GetPlasma(gorger), Is.EqualTo(100));
            Assert.That(server.EntMan.GetComponent<MCXenoTransfusionComponent>(gorger).Recipient, Is.EqualTo(gorger));
        });

        await pair.RunTicksSync(90);
        await server.WaitAssertion(() =>
        {
            Assert.That(plasma.GetPlasma(gorger), Is.EqualTo(80));
            Assert.That(server.EntMan.GetComponent<MCXenoTransfusionComponent>(gorger).Recipient, Is.Null);
            var reserve = server.EntMan.GetComponent<MCXenoOverhealComponent>(gorger);
            Assert.That((float) reserve.Amount, Is.EqualTo(180));
            var effects = server.EntMan.GetComponent<MCAuraComponent>(gorger).Effects;
            var overheal = effects.Values.Single();
            Assert.That(overheal.Preset.Id, Is.EqualTo("MCOverhealAura"));
            Assert.That(overheal.Strength, Is.EqualTo(180f / (float) reserve.Maximum).Within(0.001f));

            var carnage = xenoActions.GetActionsWithEvent<MCXenoCarnageActionEvent>(gorger).Single();
            actions.PerformAction(gorger, carnage);
            server.System<MCDamageableSystem>().AdjustBruteLoss(gorger, 100);
            Assert.That((float) reserve.Amount, Is.EqualTo(80));
            Assert.That(effects.Values.Single(effect => effect.Preset == overheal.Preset).Strength,
                Is.EqualTo(80f / (float) reserve.Maximum).Within(0.001f));
            server.System<MCDamageableSystem>().AdjustBruteLoss(gorger, 100);
            Assert.That(effects.Values.Single().Preset.Id, Is.EqualTo("MCCarnageAura"));
            Assert.That((float) reserve.Amount, Is.Zero);
            Assert.That(health.GetHealth(gorger), Is.EqualTo(580));
        });
        await server.WaitPost(() => server.System<SharedMapSystem>().DeleteMap(map.MapId));
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task TransfusionFollowsMovingHiveMembersAndStopsWhenTheBeamBreaks()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Map = "MCTalos",
            Connected = true,
            NoLoadTestPrototypes = true,
        });
        var server = pair.Server;
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        var transforms = server.System<SharedTransformSystem>();
        var damage = server.System<MCDamageableSystem>();
        var plasma = server.System<MCXenoPlasmaSystem>();
        var health = server.System<MCXenoHealSystem>();
        var actions = server.System<SharedActionsSystem>();
        EntityUid donor = default;
        EntityUid patient = default;
        float initialBlood = 0;

        EntityCoordinates At(float x, float y = 0.5f) => map.GridCoords.Offset(new Vector2(x, y));
        EntityUid? Recipient() => server.EntMan.GetComponent<MCXenoTransfusionComponent>(donor).Recipient;
        void Use(EntityUid target)
        {
            var action = server.System<SharedRMCActionsSystem>().GetActionsWithEvent<MCXenoTransfusionActionEvent>(donor).Single();
            Assert.That(actions.SetEventTarget(action, target), Is.True);
            actions.PerformAction(donor, action);
        }

        await server.WaitAssertion(() =>
        {
            var maps = server.System<SharedMapSystem>();
            for (var x = 0; x < 6; x++)
            {
                for (var y = 0; y < 2; y++)
                {
                    maps.SetTile(map.Grid, new Vector2i(x, y), map.Tile.Tile);
                }
            }

            var hive = server.EntMan.SpawnEntity("MCXenoHive", At(0.5f));
            donor = server.EntMan.SpawnEntity("MCXenoGorger", At(0.5f));
            patient = server.EntMan.SpawnEntity("MCXenoGorger", At(2.5f));
            var hives = server.System<MCSharedXenoHiveSystem>();
            hives.SetHive(donor, hive);
            hives.SetHive(patient, hive);
            plasma.DrainPlasma(donor, 25);
            initialBlood = plasma.GetPlasma(donor);
            Use(donor);
            damage.AdjustBruteLoss(donor, 10);
            damage.AdjustBruteLoss(patient, 100);
        });
        await pair.RunTicksSync(3);
        await server.WaitAssertion(() =>
        {
            Assert.That(Recipient(), Is.Null);
            Use(patient);
            Use(patient);
            Use(donor);
            Assert.That(Recipient(), Is.EqualTo(patient));
            transforms.SetCoordinates(donor, At(1.5f));
            transforms.SetCoordinates(patient, At(4.5f));
        });
        await pair.RunTicksSync(3);
        await server.WaitAssertion(() =>
        {
            Assert.That(Recipient(), Is.EqualTo(patient), "The beam reaches exactly three tiles.");
            transforms.SetCoordinates(patient, At(4.55f));
        });
        await pair.RunTicksSync(3);
        await server.WaitAssertion(() =>
        {
            Assert.That(Recipient(), Is.Null);
            transforms.SetCoordinates(patient, At(3.5f, 1.5f));
            server.EntMan.SpawnEntity("CMWallMetal", At(2.5f));
            transforms.SetCoordinates(donor, At(1.5f, 1.5f));
            Use(patient);
            Assert.That(Recipient(), Is.EqualTo(patient));
            transforms.SetCoordinates(donor, At(1.5f));
            transforms.SetCoordinates(patient, At(3.5f));
        });
        await pair.RunTicksSync(3);
        await server.WaitAssertion(() =>
        {
            Assert.That(Recipient(), Is.Null, "A wall between the moving endpoints breaks the beam.");
            Assert.That(plasma.GetPlasma(donor), Is.EqualTo(initialBlood));
            Assert.That(health.GetHealth(patient), Is.EqualTo(500));
            transforms.SetCoordinates(donor, At(1.5f, 1.5f));
            transforms.SetCoordinates(patient, At(3.5f, 1.5f));
            Use(patient);
            transforms.SetCoordinates(donor, At(0.75f, 1.5f));
        });
        await pair.RunTicksSync(90);
        await server.WaitAssertion(() =>
        {
            Assert.That(Recipient(), Is.Null);
            Assert.That(plasma.GetPlasma(donor), Is.EqualTo(initialBlood - 20));
            Assert.That(health.GetHealth(patient), Is.EqualTo(600));
        });
        await server.WaitPost(() => server.System<SharedMapSystem>().DeleteMap(map.MapId));
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task HealingInfusionOutlastsItsBeamAndStacksWithRageAndJelly()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Map = "MCTalos",
            Connected = true,
            NoLoadTestPrototypes = true,
        });
        var server = pair.Server;
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        EntityUid patient = default;
        EntityUid pulse = default;
        await server.WaitAssertion(() =>
        {
            var hive = server.EntMan.SpawnEntity("MCXenoHive", map.GridCoords);
            var caster = server.EntMan.SpawnEntity("MCXenoHivelord", map.GridCoords);
            patient = server.EntMan.SpawnEntity("MCXenoRavager", map.GridCoords.Offset(Vector2.UnitX));
            var hives = server.System<MCSharedXenoHiveSystem>();
            hives.SetHive(caster, hive);
            hives.SetHive(patient, hive);
            var rage = server.System<MCXenoRageSystem>();
            rage.Activate(patient);
            rage.SetPower(patient, 0.75f);
            var action = server.System<SharedRMCActionsSystem>().GetActionsWithEvent<MCXenoHealingInfusionActionEvent>(caster).Single();
            var actions = server.System<SharedActionsSystem>();
            actions.SetEventTarget(action, patient);
            actions.PerformAction(caster, action);

            var effects = server.EntMan.GetComponent<MCAuraComponent>(patient).Effects;
            Assert.That(effects.Values.Select(effect => effect.Preset.Id), Is.EquivalentTo(new[] { "MCHealingInfusionAura", "MCRageAura" }));
            var beams = server.EntMan.EntityQueryEnumerator<MCXenoHealingInfusionBeamComponent>();
            Assert.That(beams.MoveNext(out pulse, out var beam), Is.True);
            Assert.That(beam!.Recipient, Is.EqualTo(patient));
            Assert.That(server.EntMan.GetComponent<TransformComponent>(pulse).ParentUid, Is.EqualTo(caster));
            var auras = server.System<MCAuraSystem>();
            const string infusionLayer = "healing-infusion";
            auras.Set(patient, infusionLayer, "MCHealingInfusionAura", emitter: caster);
            Assert.That(effects.Count, Is.EqualTo(3));
            auras.Set(patient, "second-layer", "MCHealingInfusionAura", emitter: caster);
            Assert.That(effects.Count, Is.EqualTo(4));
            auras.Remove(patient, infusionLayer, caster);
            Assert.That(effects.Keys.Any(source => source.Layer == "second-layer"), Is.True);
            auras.Remove(patient, "second-layer", caster);
            Assert.That(effects.Count, Is.EqualTo(2));
            rage.Deactivate(patient);
            Assert.That(effects.Values.Single().Preset.Id, Is.EqualTo("MCHealingInfusionAura"));

            var createJelly = server.System<SharedRMCActionsSystem>().GetActionsWithEvent<MCXenoCreateResinJellyActionEvent>(caster).Single();
            actions.PerformAction(caster, createJelly);
            var jelly = server.System<SharedHandsSystem>().GetActiveItem(caster);
            Assert.That(jelly, Is.Not.Null);
            server.EntMan.EventBus.RaiseLocalEvent(jelly!.Value,
                new AfterInteractEvent(caster, jelly.Value, patient, map.GridCoords.Offset(Vector2.UnitX), true));
        });
        await pair.RunTicksSync(90);
        await server.WaitAssertion(() =>
        {
            Assert.That(server.EntMan.EntityExists(pulse), Is.False);
            var effects = server.EntMan.GetComponent<MCAuraComponent>(patient).Effects;
            Assert.That(effects.Values.Select(effect => effect.Preset.Id), Is.EquivalentTo(new[] { "MCHealingInfusionAura", "MCResinJellyAura" }));
            server.EntMan.RemoveComponent<MCXenoResinJellyFireproofComponent>(patient);
            Assert.That(effects.Values.Single().Preset.Id, Is.EqualTo("MCHealingInfusionAura"));
        });
        await server.WaitPost(() => server.System<SharedMapSystem>().DeleteMap(map.MapId));
        await pair.CleanReturnAsync();
    }

    [TestCase(false, 250)]
    [TestCase(true, 400)]
    public async Task DrainSuppliesTwoMealsAndPowersFeast(bool startFull, int expectedBlood)
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Map = "MCTalos",
            Connected = true,
            NoLoadTestPrototypes = true,
        });
        var server = pair.Server;
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        var actions = server.System<SharedActionsSystem>();
        var xenoActions = server.System<SharedRMCActionsSystem>();
        var plasma = server.System<MCXenoPlasmaSystem>();
        EntityUid gorger = default;
        await server.WaitAssertion(() =>
        {
            gorger = server.EntMan.SpawnEntity("MCXenoGorger", map.GridCoords);
            var human = server.EntMan.SpawnEntity("MCMobHuman", map.GridCoords.Offset(Vector2.UnitX));
            var drain = xenoActions.GetActionsWithEvent<MCXenoDrainActionEvent>(gorger).Single();
            if (startFull)
            {
                plasma.RegenPlasmaMax(gorger);
                Assert.That(plasma.GetPlasma(gorger), Is.EqualTo(plasma.GetMaxPlasma(gorger)));
            }

            actions.SetEventTarget(drain, human);
            actions.PerformAction(gorger, drain);
            Assert.That(server.EntMan.GetComponent<MCXenoDrainComponent>(gorger).Feeding, Is.True);
            server.System<MCDamageableSystem>().AdjustBruteLoss(gorger, 10);
        });
        await pair.RunTicksSync(60);
        await server.WaitAssertion(() =>
        {
            Assert.That(plasma.GetPlasma(gorger), Is.EqualTo(expectedBlood));
            var feast = xenoActions.GetActionsWithEvent<MCXenoFeastActionEvent>(gorger).Single();
            actions.PerformAction(gorger, feast);
        });
        await pair.RunTicksSync(30);
        await server.WaitAssertion(() => Assert.That(plasma.GetPlasma(gorger), Is.EqualTo(expectedBlood - 20)));
        await server.WaitPost(() => server.System<SharedMapSystem>().DeleteMap(map.MapId));
        await pair.CleanReturnAsync();
    }

    [Test]
    public async Task PsychicLinkSplitsDamageAndProtectsItsOwnerAtLowHealth()
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Map = "MCTalos",
            Connected = true,
            NoLoadTestPrototypes = true,
        });
        var server = pair.Server;
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        var actions = server.System<SharedActionsSystem>();
        var health = server.System<MCXenoHealSystem>();
        var damage = server.System<MCDamageableSystem>();
        var hives = server.System<MCSharedXenoHiveSystem>();
        EntityUid owner = default;
        EntityUid sister = default;
        await server.WaitAssertion(() =>
        {
            var hive = server.EntMan.SpawnEntity("MCXenoHive", map.GridCoords);
            owner = server.EntMan.SpawnEntity("MCXenoGorger", map.GridCoords);
            sister = server.EntMan.SpawnEntity("MCXenoGorger", map.GridCoords.Offset(Vector2.UnitX));
            hives.SetHive(owner, hive);
            hives.SetHive(sister, hive);
            var link = server.System<SharedRMCActionsSystem>().GetActionsWithEvent<MCXenoPsychicLinkActionEvent>(owner).Single();
            actions.SetEventTarget(link, sister);
            actions.PerformAction(owner, link);
        });
        await pair.RunTicksSync(260);
        await server.WaitAssertion(() =>
        {
            Assert.That(server.EntMan.HasComponent<MCXenoPsychicLinkTargetComponent>(sister), Is.True);
            damage.AdjustBruteLoss(sister, 20);
            Assert.That(health.GetHealth(owner), Is.EqualTo(590));
            Assert.That(health.GetHealth(sister), Is.EqualTo(590));
            damage.AdjustBruteLoss(owner, 465);
            damage.AdjustBurnLoss(sister, 20);
            Assert.That(health.GetHealth(owner), Is.EqualTo(120));
            Assert.That(health.GetHealth(sister), Is.EqualTo(575));
        });
        await pair.RunTicksSync(2);
        await server.WaitAssertion(() => Assert.That(server.EntMan.HasComponent<MCXenoPsychicLinkTargetComponent>(sister), Is.False));
        await server.WaitPost(() => server.System<SharedMapSystem>().DeleteMap(map.MapId));
        await pair.CleanReturnAsync();
    }
}
