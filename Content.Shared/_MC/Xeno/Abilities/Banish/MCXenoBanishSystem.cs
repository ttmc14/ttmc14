using Content.Shared._MC.Transform;
using Content.Shared._MC.Xeno.Abilities.Recall;
using Content.Shared._RMC14.Actions;
using Content.Shared.Examine;
using Content.Shared.Tag;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Banish;

public sealed partial class MCXenoBanishSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly MetaDataSystem _metaData = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = null!;
    [Dependency] private readonly ExamineSystemShared _examine = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly TagSystem _tag = null!;

    [Dependency] private readonly MCSharedTransformSystem _mcTransform = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoBanishComponent, MCXenoBanishActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoBanishComponent, MCXenoRecallActionEvent>(OnRecallAction);

        InitializeBanished();
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoBanishedComponent>();
        while (query.MoveNext(out var uid, out var banishedComponent))
        {
            if (_timing.CurTime < banishedComponent.EndTime)
                continue;

            RemCompDeferred<MCXenoBanishedComponent>(uid);
        }
    }

    private void OnAction(Entity<MCXenoBanishComponent> entity, ref MCXenoBanishActionEvent args)
    {
        if (args.Handled)
            return;

        var origin = _transform.GetMapCoordinates(entity);
        var target = _transform.GetMapCoordinates(args.Target);
        var distance = (origin.Position - target.Position).Length();

        if (distance > entity.Comp.Range)
            return;

        if (!_examine.InRangeUnOccluded(origin, target, entity.Comp.Range, null))
            return;

        if (_tag.HasTag(args.Target, entity.Comp.IgnoreTag))
            return;

        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
            return;

        args.Handled = true;

        if (entity.Comp.Target is { } previousTarget)
            RemCompDeferred<MCXenoBanishedComponent>(previousTarget);

        entity.Comp.Target = args.Target;
        Dirty(entity);

        var banished = EnsureComp<MCXenoBanishedComponent>(args.Target);
        banished.User = entity;
        banished.Position = _transform.GetMapCoordinates(args.Target);
        banished.EndTime = _timing.CurTime + entity.Comp.Duration;
        Dirty(args.Target, banished);

        _mcTransform.SetMapCoordinates(args.Target, new MapCoordinates(_transform.GetWorldPosition(args.Target), GetMap()), unanchor: false);
    }

    private void OnRecallAction(Entity<MCXenoBanishComponent> entity, ref MCXenoRecallActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
            return;

        args.Handled = true;

        if (entity.Comp.Target is not { } target)
            return;

        RemComp<MCXenoBanishedComponent>(target);
    }

    private MapId GetMap()
    {
        var query = EntityQueryEnumerator<MCXenoBanishMapComponent, MapComponent>();
        while (query.MoveNext(out _, out _, out var mapComponent))
        {
            return mapComponent.MapId;
        }

        var mapUid = _map.CreateMap(out var mapId);
        AddComp<MCXenoBanishMapComponent>(mapUid);

        _metaData.SetEntityName(mapUid, "Banish");

        // var parallax = EnsureComp<ParallaxComponent>(mapUid);
        // parallax.Parallax = ...Parallax;

        return mapId;
    }
}
