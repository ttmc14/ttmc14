using System.Numerics;
using Content.Shared._MC.Xeno.Projectiles;
using Content.Shared._RMC14.Weapons.Ranged;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Hive;
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
using Robust.Shared.Spawners;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Spit;

/// <summary>
/// Handles the logic for Xeno ranged projectile attacks (spits), including targeting,
/// projectile spawning, resource cost (plasma), firing delays, and applying projectile metadata.
/// </summary>
public abstract class MCSharedXenoSpitSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly IRobustRandom _random = default!;

    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedGunSystem _gun = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;

    [Dependency] private readonly XenoSystem _xeno = default!;
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;
    [Dependency] private readonly SharedXenoHiveSystem _xenoHive = default!;

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

        // TODO: add popup delay, caused spamming
        // Abort if not enough plasma
        if (!_xenoPlasma.TryRemovePlasmaPopup(xeno, comp.PlasmaCost))
            return;

        comp.NextShot = _timing.CurTime + comp.Delay;
        Dirty(xeno, comp);

        Shoot(xeno, coords, comp.ProjectileId, 1, Angle.Zero, comp.Speed, comp.Sound, null, target);
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
    /// Spawns and launches a projectile from a Xeno.
    /// </summary>
    public void Shoot(EntityUid xeno,
        EntityCoordinates targetCoords,
        EntProtoId projectileId,
        int shots,
        Angle deviation,
        float speed = 30,
        SoundSpecifier? sound = null,
        float? fixedDistance = null,
        EntityUid? target = null)
    {
        var origin = _transform.GetMapCoordinates(xeno);
        var targetMap = _transform.ToMapCoordinates(targetCoords);

        // Invalid or same-position shot
        if (origin.MapId != targetMap.MapId || origin.Position == targetMap.Position)
            return;

        _audio.PlayPredicted(sound, xeno, xeno);
        if (_net.IsClient)
            return;

        var ammoShotEvent = new AmmoShotEvent { FiredProjectiles = new(shots) };

        // Prevent friendly fire on invalid targets
        if (target is not null && HasComp<MobStateComponent>(target) && !_xeno.CanAbilityAttackTarget(xeno, target.Value))
            target = null;

        var direction = targetMap.Position - origin.Position;

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
        }

        RaiseLocalEvent(xeno, ammoShotEvent);
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
    /// Adds additional data to projectile such as lifetime or target.
    /// </summary>
    private void SetupProjectile(EntityUid projectile, EntityUid xeno, EntityUid? targetUid, float? fixedDistance, MapCoordinates originCoords, MapCoordinates targetCoords, float speed)
    {
        EnsureComp<XenoProjectileComponent>(projectile);

        // Link to hive for friendly behavior
        _xenoHive.SetSameHive(xeno, projectile);

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
            var despawn = EnsureComp<TimedDespawnComponent>(projectile);
            despawn.Lifetime = (targetCoords.Position - originCoords.Position).Length() / speed;
            Dirty(projectile, despawn);
        }
    }
}
