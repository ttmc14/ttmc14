using System.Linq;
using Content.Shared._MC.Damage;
using Content.Shared._MC.Knockback;
using Content.Shared._MC.Line;
using Content.Shared._MC.Map;
using Content.Shared._MC.Physics;
using Content.Shared._MC.Stun;
using Content.Shared._MC.Xeno.Hive.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Physics;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Warlock.Blast;

public sealed class MCXenoBlastSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedPhysicsSystem _physics = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly EntityLookupSystem _lookup = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;

    [Dependency] private readonly MCKnockbackSystem _mcKnockback = null!;
    [Dependency] private readonly MCSharedXenoHiveSystem _mcXenoHive = null!;
    [Dependency] private readonly MCStunSystem _mcStun = null!;
    [Dependency] private readonly MCLineSystem _mcLine = null!;
    [Dependency] private readonly MCDamageableSystem _mcDamageable = null!;
    [Dependency] private readonly MCPhysicsFixtureCacheSystem _mcPhysicsFixtureCache = null!;
    [Dependency] private readonly MCAnchoredRadiusSystem _mcAnchoredRadius = null!;


    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoBlastComponent, MCXenoBlastActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoBlastComponent, MCXenoBlastDoAfterEvent>(OnDoAfter);
    }

    private void OnAction(Entity<MCXenoBlastComponent> entity, ref MCXenoBlastActionEvent args)
    {
        if (args.Handled)
            return;

        if (!CanUseAction(entity, args.Action))
            return;

        var coordinatesStart = _transform.GetMapCoordinates(entity);
        var coordinatesTarget = _transform.ToMapCoordinates(args.Target);

        var direction = coordinatesTarget.Position - coordinatesStart.Position;
        var distance = float.Min(direction.Length(), entity.Comp.Range);

        if (distance <= 0)
            return;

        var ray = new CollisionRay(
            coordinatesStart.Position,
            direction.Normalized(),
            (int) CollisionGroup.Opaque | (int) CollisionGroup.BarricadeImpassable
        );

        var results = _physics.IntersectRayWithPredicate(
            _transform.GetMapId(entity.Owner),
            ray,
            distance,
            ent => ent == entity.Owner || IsDead(entity),
            false
        );

        var finalTarget = coordinatesTarget;
        foreach (var result in results)
        {
            finalTarget = new MapCoordinates(result.HitPos, coordinatesTarget.MapId);
            break;
        }

        var doAfter = new DoAfterArgs(
            EntityManager,
            entity.Owner,
            entity.Comp.Delay,
            new MCXenoBlastDoAfterEvent(args.Action, coordinatesStart, finalTarget, EntityManager),
            entity.Owner)
        {
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnDoAfter(Entity<MCXenoBlastComponent> entity, ref MCXenoBlastDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        var action = GetEntity(args.ActionUid);
        if (!TryUseAction(entity, action))
            return;

        ActionStartUseDelay<MCXenoBlastActionEvent>(entity, action);

        args.Handled = true;

        Cast(entity, args.Start, args.Target);
    }

    private void Cast(Entity<MCXenoBlastComponent> entity, MapCoordinates startCoords, MapCoordinates targetCoords)
    {
        _audio.PlayPredicted(entity.Comp.EffectSound, _transform.ToCoordinates(targetCoords), entity);
        _mcLine.SpawnEffect(entity.Comp.RayEffectId, startCoords, targetCoords);

        ServerSpawn(entity.Comp.EffectId, targetCoords);

        var mapId = startCoords.MapId;

        var entities = _lookup.GetEntitiesInRange(targetCoords, entity.Comp.EffectRange, LookupFlags.Uncontained);

        var anchoredQuery = _mcAnchoredRadius.GetAnchoredInRadius(_transform.ToCoordinates(targetCoords), (int) float.Ceiling(entity.Comp.EffectRange));
        while (anchoredQuery.MoveNext(out var anchoredUid))
        {
            entities.Add(anchoredUid);
        }

        foreach (var targetUid in entities)
        {
            // Always ignore self
            if (entity.Owner == targetUid)
                continue;

            // Ignore same hive
            if (_mcXenoHive.FromSameHive(entity.Owner, targetUid))
                continue;

            // Ignore dead entities
            if (IsMob(targetUid) && IsDead(targetUid))
                continue;

            // Ignore entities in storages etc.
            if (!IsOnMap(targetUid))
                continue;

            // Ignore god-mod entities
            if (!IsDamageable(targetUid))
                continue;

            // Ignore entities without collider
            var fixture = _mcPhysicsFixtureCache.GetFirstFixture(targetUid);
            if (fixture is null)
                continue;

            // Ignore entities with non-correct fixture layer
            // if ((fixture.CollisionLayer & (int) CollisionGroup.MobLayer) == 0)
            //    continue;

            var coords = Transform(targetUid).Coordinates;
            var direction = coords.Position - targetCoords.Position;
            var distance = direction.Length();

            if (distance <= 0)
                continue;

            var ray = new CollisionRay(
                targetCoords.Position,
                direction.Normalized(),
                (int) CollisionGroup.Impassable
            );

            if (_physics.IntersectRayWithPredicate(mapId, ray, distance, Predicate).Any())
                continue;

            _mcDamageable.DealBombDamage(targetUid, entity.Comp.ArmorPiercing, entity.Comp.Damage, origin: entity, tool: entity);
            _mcStun.Slowdown(targetUid, entity.Comp.SlowdownDuration);
            _mcKnockback.Knockback(targetUid, direction, entity.Comp.KnockbackEntry);

            RaiseEffect(entity, targetUid);
        }

        return;

        bool Predicate(EntityUid uid)
        {
            return IsMob(uid);
        }
    }
}
