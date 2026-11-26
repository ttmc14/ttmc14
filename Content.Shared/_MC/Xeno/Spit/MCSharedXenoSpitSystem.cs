using System.Numerics;
using Content.Shared._MC.Weapon;
using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._MC.Xeno.Projectiles;
using Content.Shared._MC.ZLevels.Weapons;
using Content.Shared._RMC14.Projectiles;
using Content.Shared._RMC14.Weapons.Ranged;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared._RMC14.Xenonids.Projectile;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Physics.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Spit;

/// <summary>
/// Handles the logic for Xeno ranged projectile attacks (spits), including targeting,
/// projectile spawning, resource cost (plasma), firing delays, and applying projectile metadata.
/// </summary>
public abstract class MCSharedXenoSpitSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly IRobustRandom _random = null!;

    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedGunSystem _gun = null!;
    [Dependency] private readonly SharedPhysicsSystem _physics = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    [Dependency] private readonly XenoSystem _rmcXeno = null!;
    [Dependency] private readonly RMCProjectileSystem _rmcProjectile = null!;
    [Dependency] private readonly XenoPlasmaSystem _rmcXenoPlasma = null!;

    [Dependency] private readonly MCSharedXenoHiveSystem _mcXenoHive = null!;
    [Dependency] private readonly MCZLevelShootingSystem _zShooting = null!;
    [Dependency] private readonly MCZLevelShootHelperSystem _mcZHelper = null!;

    protected EntityQuery<MCXenoSpitComponent> XenoSpitQuery;

    public override void Initialize()
    {
        base.Initialize();

        XenoSpitQuery = GetEntityQuery<MCXenoSpitComponent>();

        SubscribeAllEvent<MCXenoSpitEvent>(OnSpit);
    }

    /// <summary>
    /// Handles spit input event from player.
    /// </summary>
    private void OnSpit(MCXenoSpitEvent ev)
    {
        var xeno = GetEntity(ev.Xeno);
        var target = GetEntity(ev.Target);
        var coords = GetCoordinates(ev.Coordinates);

        // Abort if no valid component or not ready
        if (!XenoSpitQuery.TryComp(xeno, out var comp) || !comp.Enabled || comp.NextShot > _timing.CurTime)
            return;

        if (!TryGetShotCoordinates(xeno, coords, out var origin, out var destination))
            return;

        // TODO: add popup delay, caused spamming
        // Abort if not enough plasma
        if (!_rmcXenoPlasma.TryRemovePlasmaPopup(xeno, comp.PlasmaCost))
            return;

        comp.NextShot = _timing.CurTime + comp.Delay;
        Dirty(xeno, comp);

        Shoot(xeno, origin, destination, comp.ProjectileId, 1, Angle.Zero, comp.Speed, comp.Sound, target: target);
    }

    /// <summary>
    /// Sets up the spit preset for a Xeno.
    /// </summary>
    public void SetPreset(Entity<MCXenoSpitComponent?> entity, EntProtoId projectileId, FixedPoint2 plasmaCost, TimeSpan delay, float speed, SoundSpecifier? sound)
    {
        entity.Comp ??= EnsureComp<MCXenoSpitComponent>(entity);

        entity.Comp.Enabled = true;
        entity.Comp.ProjectileId = projectileId;
        entity.Comp.PlasmaCost = plasmaCost;
        entity.Comp.Delay = delay;
        entity.Comp.Speed = speed;
        entity.Comp.Sound = sound;
        Dirty(entity);
    }

    /// <summary>
    /// Resets and disables the spit preset for a Xeno.
    /// </summary>
    public void ResetPreset(Entity<MCXenoSpitComponent?> entity)
    {
        entity.Comp ??= EnsureComp<MCXenoSpitComponent>(entity);

        entity.Comp.Enabled = false;
        Dirty(entity);
    }

    /// <summary>
    /// Resolves a spit before the caller spends plasma or ammunition.
    /// </summary>
    public bool TryGetShotCoordinates(EntityUid xeno, EntityCoordinates aim, out MapCoordinates origin, out MapCoordinates target)
    {
        return _zShooting.TryGetAbilityShotCoordinates(xeno, aim, out origin, out target) && origin.Position != target.Position;
    }

    /// <summary>
    /// Fires at a clicked point using the shooter's current Z-level view.
    /// </summary>
    public EntityUid[] Shoot(EntityUid xeno, EntityCoordinates targetCoords, EntProtoId projectileId,
        int shots, Angle deviation, float speed = 30, SoundSpecifier? sound = null, float? fixedDistance = null, EntityUid? target = null)
    {
        if (!TryGetShotCoordinates(xeno, targetCoords, out var origin, out var destination))
            return [];

        return Shoot(xeno, origin, destination, projectileId, shots, deviation, speed, sound, fixedDistance, target);
    }

    /// <summary>
    /// Launches projectiles along a resolved spit trajectory.
    /// </summary>
    public EntityUid[] Shoot(EntityUid xeno,
        MapCoordinates origin,
        MapCoordinates targetMap,
        EntProtoId projectileId,
        int shots,
        Angle deviation,
        float speed = 30,
        SoundSpecifier? sound = null,
        float? fixedDistance = null,
        EntityUid? target = null)
    {
        _audio.PlayPredicted(sound, xeno, xeno);

        if (_net.IsClient)
            return [];

        var ammoShotEvent = new AmmoShotEvent { FiredProjectiles = new List<EntityUid>(shots) };

        // Prevent friendly fire on invalid targets
        if (target is { } uid && (_transform.GetMapId(uid) != targetMap.MapId ||
            HasComp<MobStateComponent>(uid) && !_rmcXeno.CanAbilityAttackTarget(xeno, uid)))
        {
            target = null;
        }

        var result = new EntityUid[shots];
        var direction = targetMap.Position - origin.Position;
        deviation += _gun.GetShooterSpread(xeno);
        for (var i = 0; i < shots; i++)
        {
            var projTarget = ApplyDeviation(targetMap, deviation, direction);
            var diff = projTarget.Position - origin.Position;
            var velocity = _physics.GetMapLinearVelocity(xeno);
            diff *= speed / diff.Length();

            var projectile = Spawn(projectileId, origin);
            SetupProjectile(projectile, xeno, target, fixedDistance, origin, targetMap, speed);
            _gun.ShootProjectile(projectile, diff, velocity, xeno, xeno, speed);
            ammoShotEvent.FiredProjectiles.Add(projectile);

            result[i] = projectile;
        }

        if (origin.MapId == Transform(xeno).MapID)
        {
            _mcZHelper.ApplyZPhysics(xeno, ammoShotEvent.FiredProjectiles, _transform.ToCoordinates(targetMap), speed);
        }

        RaiseLocalEvent(xeno, ammoShotEvent);
        return result;
    }

    /// <summary>
    /// Randomizes target direction within deviation angle.
    /// </summary>
    private MapCoordinates ApplyDeviation(MapCoordinates targetCoords, Angle deviation, Vector2 direction)
    {
        if (deviation == Angle.Zero)
            return targetCoords;

        var angle = _random.NextAngle(-deviation / 2, deviation / 2);
        return new MapCoordinates(targetCoords.Position + angle.RotateVec(direction), targetCoords.MapId);
    }

    /// <summary>
    /// Configures the projectile's range, target and hive.
    /// </summary>
    private void SetupProjectile(EntityUid projectile, EntityUid xeno, EntityUid? targetUid, float? fixedDistance, MapCoordinates originCoords, MapCoordinates targetCoords, float speed)
    {
        EnsureComp<XenoProjectileComponent>(projectile);

        // Link to hive for friendly behavior
        _mcXenoHive.SetSameHive(xeno, projectile);

        if (fixedDistance is not null)
        {
            var fixedComp = EnsureComp<ProjectileFixedDistanceComponent>(projectile);
            fixedComp.FlyEndTime = _timing.CurTime + TimeSpan.FromSeconds(fixedDistance.Value / speed);
            Dirty(projectile, fixedComp);
        }

        if (targetUid is not null)
        {
            var targeted = EnsureComp<TargetedProjectileComponent>(projectile);
            targeted.Target = targetUid.Value;
            Dirty(projectile, targeted);
        }

        if (HasComp<MCXenoProjectileTargetingTurfComponent>(projectile))
        {
            var distance = Vector2.Distance(originCoords.Position, targetCoords.Position);
            if (TryComp<ProjectileMaxRangeComponent>(projectile, out var range))
            {
                distance = MathF.Min(distance, range.Max);
            }

            _rmcProjectile.SetMaxRange(projectile, distance);
        }
    }
}
