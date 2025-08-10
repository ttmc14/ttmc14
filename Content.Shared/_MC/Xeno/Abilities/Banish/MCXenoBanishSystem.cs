using Content.Shared._MC.Xeno.Abilities.Recall;
using Content.Shared._RMC14.Actions;
using Content.Shared.Bed.Sleep;
using Content.Shared.Examine;
using Content.Shared.StatusEffectNew;
using Content.Shared.Tag;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Banish;

public sealed class MCXenoBanishSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly MetaDataSystem _metaData = default!;
    [Dependency] private readonly SharedMapSystem _map = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SleepingSystem _sleeping = default!;
    [Dependency] private readonly SharedStatusEffectsSystem _statusEffects = default!;
    [Dependency] private readonly TagSystem _tag = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoBanishComponent, MCXenoBanishActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoBanishComponent, MCXenoRecallActionEvent>(OnRecallAction);

        SubscribeLocalEvent<MCXenoBanishedComponent, ComponentShutdown>(OnShutdown);
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

        _statusEffects.TryAddStatusEffectDuration(args.Target, "StatusEffectForcedSleeping", TimeSpan.FromHours(1));

        _transform.SetMapCoordinates(args.Target, new MapCoordinates(_transform.GetWorldPosition(args.Target), GetMap()));
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

        RemCompDeferred<MCXenoBanishedComponent>(target);
    }

    private void OnShutdown(Entity<MCXenoBanishedComponent> entity, ref ComponentShutdown args)
    {
        if (Exists(entity.Comp.User) && TryComp<MCXenoBanishComponent>(entity.Comp.User, out var userBanishComponent))
        {
            userBanishComponent.Target = null;
            Dirty(entity.Comp.User, userBanishComponent);
        }

        _transform.SetMapCoordinates(entity, entity.Comp.Position);

        _statusEffects.TryRemoveStatusEffect(entity, "StatusEffectForcedSleeping");

        _sleeping.TryWaking(entity.Owner, true);
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
