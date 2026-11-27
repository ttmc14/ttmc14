using Content.Server.GameTicking;
using Content.Server.GameTicking.Rules;
using Content.Server.RoundEnd;
using Content.Shared._MC.Nuke.Generator.Components;
using Content.Shared._MC.Rules.Utilities;
using Content.Shared.GameTicking.Components;
using Robust.Server.GameObjects;
using Robust.Shared.Map;
using Robust.Shared.Random;

namespace Content.Server._MC.Rules.Utilities;

public sealed class MCRuleSpawnNukeDiskGeneratorsSystem : GameRuleSystem<MCRuleSpawnNukeDiskGeneratorsComponent>
{
    [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] private readonly TransformSystem _transform = null!;
    [Dependency] private readonly RoundEndSystem _roundEnd = null!;

    [Dependency] private readonly MCRelayedSystem _mcRelayed = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<RulePlayerSpawningEvent>(OnRulePlayerSpawning);
    }

    protected override void AppendRoundEndText(
        EntityUid uid,
        MCRuleSpawnNukeDiskGeneratorsComponent component,
        GameRuleComponent gameRule,
        ref RoundEndTextAppendEvent args)
    {
        base.AppendRoundEndText(uid, component, gameRule, ref args);

        if (component.Spawned)
            return;

        args.AddLine(Loc.GetString("mc-nuke-generator-spawn-failed"));
    }

    private void OnRulePlayerSpawning(RulePlayerSpawningEvent ev)
    {
        var query = QueryActiveRules();
        while (query.MoveNext(out var uid, out _, out var component, out var gameRule))
        {
            if (!GameTicker.IsGameRuleAdded(uid, gameRule))
                continue;

            _mcRelayed.Relay(() =>
            {
                RulePlayerSpawning((uid, component));
            });
        }
    }

    private void RulePlayerSpawning(Entity<MCRuleSpawnNukeDiskGeneratorsComponent> entity)
    {
        if (entity.Comp.Spawned || TerminatingOrDeleted(entity))
            return;

        var locations = new HashSet<MapCoordinates>();

        var query = EntityQueryEnumerator<MCNukeDiskGeneratorSpawnerComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            locations.Add(_transform.GetMapCoordinates(uid));
        }

        if (locations.Count < entity.Comp.EntityIds.Count)
        {
            Log.Error($"Found {locations.Count} distinct disk generator locations for {entity.Comp.EntityIds.Count} generators.");
            _roundEnd.EndRound();
            return;
        }

        var coordinates = new List<MapCoordinates>(locations);
        foreach (var protoId in entity.Comp.EntityIds)
        {
            Spawn(protoId, _random.PickAndTake(coordinates));
        }

        entity.Comp.Spawned = true;
        Dirty(entity);
    }
}
