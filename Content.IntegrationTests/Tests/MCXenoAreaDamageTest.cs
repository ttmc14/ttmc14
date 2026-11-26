using System.Linq;
using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared.Actions;
using Content.Shared.Damage;
using Robust.Shared.GameObjects;
using Robust.Shared.Map;

namespace Content.IntegrationTests.Tests;

[TestFixture]
public sealed class MCXenoAreaDamageTest
{
    private static readonly string[][] Abilities =
    [
        ["MCXenoRunnerMelter", "MCActionXenoMelterShroud"],
        ["MCXenoChimera", "MCActionXenoWarpBlast"],
        ["MCXenoWarlock", "MCActionXenoPsychicBlast"],
        ["MCXenoWarlock", "MCActionXenoPsychicCrush"],
    ];

    [TestCaseSource(nameof(Abilities))]
    public async Task AreaAttackDamagesMarineAndBarricadeWhileSparingCatwalkAndCable(string caste, string ability)
    {
        await using var pair = await PoolManager.GetServerClient(new PoolSettings
        {
            Map = "MCTalos",
            NoLoadTestPrototypes = true,
        });
        var server = pair.Server;
        var entities = server.EntMan;
        var map = await pair.CreateTestMap(tile: "CMFloorPlating");
        var target = new EntityCoordinates(map.Grid, 0.5f, 0.5f);
        var origin = new EntityCoordinates(map.Grid, -0.5f, 0.5f);
        var maps = server.System<SharedMapSystem>();
        var actions = server.System<SharedActionsSystem>();
        EntityUid catwalk = default;
        EntityUid cable = default;
        EntityUid barricade = default;
        EntityUid marine = default;

        await server.WaitAssertion(() =>
        {
            maps.SetTile(map.Grid, map.Grid.Comp, origin, map.Tile.Tile);
            catwalk = entities.SpawnEntity("CMCatwalk", target);
            cable = entities.SpawnEntity("RMCCableHeavy", target);
            barricade = entities.SpawnEntity("MCBarricadeMetal", target);
            marine = entities.SpawnEntity("MCMobHuman", target);
            var xeno = entities.SpawnEntity(caste, origin);
            var hive = entities.SpawnEntity("MCXenoHive", origin);
            server.System<MCSharedXenoHiveSystem>().SetHive(xeno, hive);

            var action = actions.GetActions(xeno)
                .Single(entry => entities.GetComponent<MetaDataComponent>(entry).EntityPrototype?.ID == ability);
            if (actions.GetEvent(action) is WorldTargetActionEvent worldTarget)
            {
                worldTarget.Target = new EntityCoordinates(map.Grid, 0.75f, 0.5f);
            }

            actions.PerformAction(xeno, action);
        });

        await pair.RunSeconds(3);
        await server.WaitAssertion(() =>
        {
            Assert.That(maps.GetTileRef(map.Grid, target).Tile, Is.EqualTo(map.Tile.Tile), "Floor");
            Assert.That(entities.GetComponent<DamageableComponent>(catwalk).TotalDamage.Float(), Is.Zero, "Catwalk");
            Assert.That(entities.GetComponent<DamageableComponent>(cable).TotalDamage.Float(), Is.Zero, "Cable");
            Assert.That(entities.GetComponent<DamageableComponent>(barricade).TotalDamage.Float(), Is.Positive, "Barricade");
            Assert.That(entities.GetComponent<DamageableComponent>(marine).TotalDamage.Float(), Is.Positive, "Marine");
        });
        await pair.CleanReturnAsync();
    }
}
