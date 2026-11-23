using System.Linq;
using System.Numerics;
using Content.Shared._MC.Xeno.Abilities.Forger.Whirlwind.Components;
using Content.Shared._RMC14.Atmos;
using Content.Shared._RMC14.Xenonids.Construction;
using Content.Shared.Damage;
using Content.Shared.Damage.Components;
using Content.Shared.Maps;
using Content.Shared.Mobs.Components;
using Content.Shared.Physics;
using Content.Shared.Tag;
using Robust.Shared.Map;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;

namespace Content.Shared._MC.Xeno.Abilities.Forger.Whirlwind;

public sealed partial class MCXenoWhirlwindProjectileSystem : MCXenoAbilitySystem
{
    private const float FlightInterval = 0.05f;
    private const CollisionGroup Obstacles = CollisionGroup.Impassable | CollisionGroup.HighImpassable;

    private static readonly ProtoId<TagPrototype> WallTag = "Wall";

    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedPhysicsSystem _physics = null!;
    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly TurfSystem _turf = null!;
    [Dependency] private readonly EntityLookupSystem _lookup = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly SharedRMCFlammableSystem _rmcFlammable = null!;
    [Dependency] private readonly IRobustRandom _random = null!;
    [Dependency] private readonly TagSystem _tag = null!;

    private readonly HashSet<Entity<MobStateComponent>> _touching = new();
    private float _accumulatedTime;

    public EntityUid Launch(EntProtoId<MCXenoWhirlwindProjectileComponent> prototype, EntityUid creator, EntityCoordinates target)
    {
        var transform = Transform(creator);
        var targetMap = _transform.ToMapCoordinates(target, logError: false);

        if (targetMap.MapId != transform.MapID)
            return EntityUid.Invalid;

        var origin = transform.Coordinates;
        var offset = _transform.ToCoordinates(origin.EntityId, targetMap).Position - origin.Position;
        var direction = offset == Vector2.Zero ? transform.LocalRotation.ToWorldVec() : Vector2.Normalize(offset);

        var uid = Spawn(prototype, origin);
        var component = Comp<MCXenoWhirlwindProjectileComponent>(uid);
        component.Destination = origin.Offset(direction * component.MaxRange);

        MCXenoHive.SetSameHive(creator, uid);
        BurnContacts((uid, component), origin);

        return uid;
    }

    public override void Update(float frameTime)
    {
        if (Net.IsClient)
            return;

        _accumulatedTime += frameTime;

        while (_accumulatedTime >= FlightInterval)
        {
            _accumulatedTime -= FlightInterval;
            UpdateWhirlwinds();
        }
    }

    private void UpdateWhirlwinds()
    {
        var query = EntityQueryEnumerator<MCXenoWhirlwindProjectileComponent, TransformComponent>();

        while (query.MoveNext(out var uid, out var component, out var transform))
        {
            if (EntityManager.IsQueuedForDeletion(uid))
                continue;

            UpdateFlight((uid, component), transform);
        }
    }

    private void UpdateFlight(Entity<MCXenoWhirlwindProjectileComponent> entity, TransformComponent transform)
    {
        var destination = entity.Comp.Destination;

        if (!destination.IsValid(EntityManager) || _transform.GetMapId(destination) != transform.MapID)
        {
            QueueDel(entity);
            return;
        }

        var start = _transform.GetMapCoordinates(entity, transform);
        var offset = _transform.ToMapCoordinates(destination).Position - start.Position;
        var distance = offset.Length();

        if (distance == 0f)
        {
            QueueDel(entity);
            return;
        }

        var direction = offset / distance;
        var step = MathF.Min(distance, entity.Comp.Speed * FlightInterval);

        if (TryHitObstacle(entity, start, direction, step))
        {
            QueueDel(entity);
            return;
        }

        var next = _transform.ToCoordinates(destination.EntityId, start.Offset(direction * step));
        MoveWhirlwind(entity, transform.Coordinates, next);

        if (step >= distance)
            QueueDel(entity);
    }

    private bool TryHitObstacle(Entity<MCXenoWhirlwindProjectileComponent> entity, MapCoordinates start, Vector2 direction, float step)
    {
        var ray = new CollisionRay(start.Position, direction, (int) (Obstacles | CollisionGroup.BarricadeImpassable));
        var hits = _physics.IntersectRayWithPredicate(start.MapId, ray, step, IsPassable, returnOnFirstHit: false);

        foreach (var hit in hits)
        {
            var impact = new MapCoordinates(hit.HitPos, start.MapId);
            var coordinates = _transform.ToCoordinates(entity.Comp.Destination.EntityId, impact);
            _transform.SetCoordinates(entity, coordinates);
            BurnContacts(entity, coordinates);

            if (!HasComp<XenoConstructComponent>(hit.HitEntity))
                _damageable.TryChangeDamage(hit.HitEntity, entity.Comp.HitDamage, origin: entity, tool: entity);

            return true;
        }

        return false;
    }

    private void MoveWhirlwind(Entity<MCXenoWhirlwindProjectileComponent> entity, EntityCoordinates previous, EntityCoordinates next)
    {
        _transform.SetCoordinates(entity, next);
        BurnContacts(entity, next);

        if (_turf.GetTileRef(next) is { } tile && tile != _turf.GetTileRef(previous))
            SpreadFire(entity, tile);
    }

    private bool IsPassable(EntityUid uid) =>
        IsMob(uid) || HasComp<XenoConstructComponent>(uid) && !_tag.HasTag(uid, WallTag);

    private void BurnContacts(Entity<MCXenoWhirlwindProjectileComponent> entity, EntityCoordinates coordinates)
    {
        _touching.Clear();
        _lookup.GetEntitiesInRange(coordinates, entity.Comp.ContactRadius, _touching, LookupFlags.Dynamic | LookupFlags.Static);

        entity.Comp.Contacts.IntersectWith(_touching);

        foreach (var mob in _touching.Where(mob => entity.Comp.Contacts.Add(mob)))
        {
            BurnMob(entity, mob);
        }
    }

    private void BurnMob(Entity<MCXenoWhirlwindProjectileComponent> entity, EntityUid mob)
    {
        if (IsXeno(mob) || IsDead(mob) || HasComp<GodmodeComponent>(mob))
            return;

        _damageable.TryChangeDamage(mob, entity.Comp.HitDamage, origin: entity, tool: entity);

        if (!_rmcFlammable.CanBeIgnited(mob, entity, entity.Comp.FireIntensity, directHit: true))
            return;

        MCFlammable.AdjustFireStacks(mob, entity.Comp.FireIntensity, entity.Comp.FireDuration, entity.Comp.FireStacks, ignite: true);
    }
}
