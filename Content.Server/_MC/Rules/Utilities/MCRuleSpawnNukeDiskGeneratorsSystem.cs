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

        args.AddLine("Internal server error. Unable to create all nuclear disk generators; the mode is partially or completely unplayable. The round has ended prematurely. Please report this to the developers; the error has been logged to the console and saved.");
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
        var coordinates = new List<MapCoordinates>();

        var query = EntityQueryEnumerator<MCNukeDiskGeneratorSpawnerComponent>();
        while (query.MoveNext(out var uid, out _))
        {
            coordinates.Add(_transform.GetMapCoordinates(uid));
        }

        foreach (var protoId in entity.Comp.EntityIds)
        {
            if (coordinates.Count == 0)
            {
                Log.Error($"Failed to spawn {protoId}, no available coordinates. Ensure that ${nameof(MCNukeDiskGeneratorSpawnerComponent)} exists on the map.");

                _roundEnd.EndRound();

                break;
            }

            Spawn(protoId, _random.PickAndTake(coordinates));
        }

        entity.Comp.Spawned = true;
        Dirty(entity);
    }
}
