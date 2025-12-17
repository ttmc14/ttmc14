using System.Numerics;
using Content.Shared._RMC14.Evasion;
using Content.Shared._RMC14.Random;
using Content.Shared._RMC14.Weapons.Ranged.Prediction;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Examine;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs.Systems;
using Content.Shared.NPC.Systems;
using Content.Shared.Popups;
using Content.Shared.Maps;
using Content.Shared.Interaction;
using Content.Shared.Projectiles;
using Content.Shared.Whitelist;
using Robust.Shared.Network;
using Robust.Shared.Map;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics.Events;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Timing;
using static Content.Shared.Physics.CollisionGroup;

namespace Content.Shared._RMC14.Projectiles;

public sealed class RMCProjectileSystem : EntitySystem
{
    [Dependency] private readonly ExamineSystemShared _examine = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly NpcFactionSystem _npcFaction = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityWhitelistSystem _whitelist = default!;
    [Dependency] private readonly SharedXenoHiveSystem _hive = default!;
    [Dependency] private readonly TurfSystem _turf = default!;
    [Dependency] private readonly SharedInteractionSystem _interaction = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;


    public override void Initialize()
    {
        SubscribeLocalEvent<DeleteOnCollideComponent, StartCollideEvent>(OnDeleteOnCollideStartCollide);
        SubscribeLocalEvent<ModifyTargetOnHitComponent, ProjectileHitEvent>(OnModifyTargetOnHit);
        SubscribeLocalEvent<ProjectileMaxRangeComponent, MapInitEvent>(OnProjectileMaxRangeMapInit);

        SubscribeLocalEvent<RMCProjectileDamageFalloffComponent, MapInitEvent>(OnFalloffProjectileMapInit);
        SubscribeLocalEvent<RMCProjectileDamageFalloffComponent, ProjectileHitEvent>(OnFalloffProjectileHit);

        SubscribeLocalEvent<RMCProjectileAccuracyComponent, MapInitEvent>(OnProjectileAccuracyMapInit);
        SubscribeLocalEvent<RMCProjectileAccuracyComponent, PreventCollideEvent>(OnProjectileAccuracyPreventCollide);

        SubscribeLocalEvent<SpawnOnTerminateComponent, MapInitEvent>(OnSpawnOnTerminatingMapInit);
        SubscribeLocalEvent<SpawnOnTerminateComponent, EntityTerminatingEvent>(OnSpawnOnTerminatingTerminate);
        SubscribeLocalEvent<SpawnOnTerminateRangeComponent, EntityTerminatingEvent>(OnSpawnInRangeOnTerminatingTerminate);

        SubscribeLocalEvent<PreventCollideWithDeadComponent, PreventCollideEvent>(OnPreventCollideWithDead);
    }

    private void OnDeleteOnCollideStartCollide(Entity<DeleteOnCollideComponent> ent, ref StartCollideEvent args)
    {
        if (_net.IsServer)
            QueueDel(ent);
    }

    private void OnModifyTargetOnHit(Entity<ModifyTargetOnHitComponent> ent, ref ProjectileHitEvent args)
    {
        if (!_whitelist.IsWhitelistPassOrNull(ent.Comp.Whitelist, args.Target))
            return;
        if (ent.Comp.Add is { } add)
            EntityManager.AddComponents(args.Target, add);
    }

    private void OnProjectileMaxRangeMapInit(Entity<ProjectileMaxRangeComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.Origin = _transform.GetMoverCoordinates(ent);
        Dirty(ent);
    }

    private void OnFalloffProjectileMapInit(Entity<RMCProjectileDamageFalloffComponent> projectile, ref MapInitEvent args)
    {
        projectile.Comp.ShotFrom = _transform.GetWorldPosition(projectile.Owner);
        Dirty(projectile);
    }

    private void OnFalloffProjectileHit(Entity<RMCProjectileDamageFalloffComponent> projectile, ref ProjectileHitEvent args)
    {
        if (projectile.Comp.ShotFrom == null || projectile.Comp.MinRemainingDamageMult < 0)
            return;

        var distance = (_transform.GetWorldPosition(args.Target) - projectile.Comp.ShotFrom.Value).Length();
        var minDamage = args.Damage.GetTotal() * projectile.Comp.MinRemainingDamageMult;
        foreach (var threshold in projectile.Comp.Thresholds)
        {
            var pastEffectiveRange = distance - threshold.Range;

            if (pastEffectiveRange <= 0)
                continue;

            var totalDamage = args.Damage.GetTotal();
            if (totalDamage <= minDamage)
                break;

            var extraMult = threshold.IgnoreModifiers ? 1 : projectile.Comp.WeaponMult;
            var minMult = FixedPoint2.Min(minDamage / totalDamage, 1);

            args.Damage *= FixedPoint2.Clamp((totalDamage - pastEffectiveRange * threshold.Falloff * extraMult) / totalDamage, minMult, 1);
        }
    }

    public void SetProjectileFalloffWeaponMult(Entity<RMCProjectileDamageFalloffComponent> projectile, FixedPoint2 mult)
    {
        projectile.Comp.WeaponMult = mult;
        Dirty(projectile);
    }

    private void OnProjectileAccuracyMapInit(Entity<RMCProjectileAccuracyComponent> projectile, ref MapInitEvent args)
    {
        projectile.Comp.ShotFrom = _transform.GetWorldPosition(projectile.Owner);
        projectile.Comp.Tick = _timing.CurTick.Value;

        Dirty(projectile);
    }

    private void OnProjectileAccuracyPreventCollide(Entity<RMCProjectileAccuracyComponent> projectile, ref PreventCollideEvent args)
    {
        if (args.Cancelled)
            return;

        if (projectile.Comp.ForceHit || projectile.Comp.ShotFrom == null)
            return;

        if (!TryComp(projectile.Owner, out ProjectileComponent? projectileComponent))
            return;

        if (!TryComp(args.OtherEntity, out EvasionComponent? evasionComponent))
            return;

        var accuracy = projectile.Comp.Accuracy;
        var targetCoords = _transform.GetWorldPosition(args.OtherEntity);
        var distance = (targetCoords - projectile.Comp.ShotFrom.Value).Length();

        foreach (var threshold in projectile.Comp.Thresholds)
        {
            var pastRange = distance - threshold.Range;

            if (threshold.Buildup)
            {
                if (pastRange >= 0)
                    continue;

                accuracy += threshold.Falloff * pastRange;
                continue;
            }

            if (pastRange <= 0)
                continue;

            accuracy -= threshold.Falloff * pastRange;
        }

        var mapId = _transform.GetMapId(args.OtherEntity);
        var shootCoordinates = new MapCoordinates(projectile.Comp.ShotFrom.Value, mapId);
        var targetCoordinates = new MapCoordinates(targetCoords, mapId);
        if (!_examine.InRangeUnOccluded(shootCoordinates, targetCoordinates, distance, null))
            accuracy += (int)AccuracyModifiers.TargetOccluded;

        if (!projectile.Comp.IgnoreFriendlyEvasion && IsProjectileTargetFriendly(projectile.Owner, args.OtherEntity))
            accuracy -= evasionComponent.ModifiedEvasionFriendly;

        accuracy -= evasionComponent.ModifiedEvasion;

        accuracy = accuracy > projectile.Comp.MinAccuracy ? accuracy : projectile.Comp.MinAccuracy;

        var random = new Xoshiro128P(projectile.Comp.GunSeed, (long)projectile.Comp.Tick << 32 | GetNetEntity(args.OtherEntity).Id).NextFloat(0f, 100f);

        if (accuracy >= random)
            return;

        args.Cancelled = true;
    }

    private bool IsProjectileTargetFriendly(EntityUid projectile, EntityUid target)
    {
        if (!TryComp(projectile, out ProjectileComponent? projectileComp) || projectileComp.Shooter == null)
            return false;

        return _npcFaction.IsEntityFriendly(projectileComp.Shooter.Value, target);
    }

    private void OnSpawnOnTerminatingMapInit(Entity<SpawnOnTerminateComponent> ent, ref MapInitEvent args)
    {
        ent.Comp.Origin = _transform.GetMoverCoordinates(ent);
        Dirty(ent);
    }

    private void OnSpawnOnTerminatingTerminate(Entity<SpawnOnTerminateComponent> ent, ref EntityTerminatingEvent args)
    {
        if (_net.IsClient)
            return;

        if (!TryComp(ent, out TransformComponent? transform))
            return;

        if (TerminatingOrDeleted(transform.ParentUid))
            return;

        var coordinates = transform.Coordinates;
        if (ent.Comp.ProjectileAdjust &&
            ent.Comp.Origin is { } origin &&
            coordinates.TryDelta(EntityManager, _transform, origin, out var delta) &&
            delta.Length() > 0)
        {
            coordinates = coordinates.Offset(delta.Normalized() / -2);

            if (HasComp<RMCFireProjectileComponent>(ent))
            {
                coordinates = coordinates.Offset(delta.Normalized()); // Apparently that works...
            }
        }

        var spawn = SpawnAtPosition(ent.Comp.Spawn, coordinates);
        _hive.SetSameHive(ent.Owner, spawn);

        if (ent.Comp.Popup is { } popup)
            _popup.PopupCoordinates(Loc.GetString(popup), coordinates, ent.Comp.PopupType ?? PopupType.Small);
    }

    private void OnSpawnInRangeOnTerminatingTerminate(Entity<SpawnOnTerminateRangeComponent> ent, ref EntityTerminatingEvent args)
    {
        if (_net.IsClient)
            return;

        if (!TryComp(ent, out TransformComponent? transform))
            return;

        if (TerminatingOrDeleted(transform.ParentUid))
            return;

        var center = transform.Coordinates;

        if (ent.Comp.ProjectileAdjust &&
            ent.Comp.Origin is { } origin &&
            center.TryDelta(EntityManager, _transform, origin, out var delta) &&
            delta.Length() > 0)
        {
            center = center.Offset(delta.Normalized() / -2);
        }

        for (var x = ent.Comp.PositionXOne; x <= ent.Comp.PositionXTwo; x++)
        {
            for (var y = ent.Comp.PositionYOne; y <= ent.Comp.PositionYTwo; y++)
            {
                var offset = new Vector2(x, y);
                var targetPos = center.Offset(offset);

                if (!CanPlaceTile(targetPos))
                    continue;

                if (!_interaction.InRangeUnobstructed(ent.Owner, targetPos, ent.Comp.Range))
                    continue;

                var spawn = SpawnAtPosition(ent.Comp.Spawn, targetPos);
                _hive.SetSameHive(ent.Owner, spawn);
            }
        }
    }

    private void OnPreventCollideWithDead(Entity<PreventCollideWithDeadComponent> ent, ref PreventCollideEvent args)
    {
        if (args.Cancelled)
            return;

        if (_mobState.IsDead(args.OtherEntity))
            args.Cancelled = true;
    }

    public void SetMaxRange(Entity<ProjectileMaxRangeComponent> ent, float max)
    {
        ent.Comp.Max = max;
        Dirty(ent);
    }

    private void StopProjectile(Entity<ProjectileMaxRangeComponent> ent)
    {
        if (ent.Comp.Delete)
        {
            if (_net.IsServer)
                QueueDel(ent);
        }
        else
        {
            _physics.SetLinearVelocity(ent, Vector2.Zero);
            RemCompDeferred<ProjectileMaxRangeComponent>(ent);
        }
    }

    public override void Update(float frameTime)
    {
        if (_net.IsClient)
            return;

        var maxQuery = EntityQueryEnumerator<ProjectileMaxRangeComponent>();
        while (maxQuery.MoveNext(out var uid, out var comp))
        {
            var coordinates = _transform.GetMoverCoordinates(uid);
            if (comp.Origin is not { } origin ||
                !coordinates.TryDistance(EntityManager, _transform, origin, out var distance))
            {
                StopProjectile((uid, comp));
                continue;
            }

            if (distance < comp.Max && Math.Abs(distance - comp.Max) > 0.1f)
                continue;

            StopProjectile((uid, comp));
        }
    }

    private bool CanPlaceTile(EntityCoordinates coords)
    {
        if (_transform.GetGrid(coords) is not { } gridId ||
            !TryComp<MapGridComponent>(gridId, out var grid))
            return false;

        var tile = _mapSystem.TileIndicesFor(gridId, grid, coords);
        return !_turf.IsTileBlocked(gridId, tile, Impassable | MidImpassable | HighImpassable, grid);
    }
}
