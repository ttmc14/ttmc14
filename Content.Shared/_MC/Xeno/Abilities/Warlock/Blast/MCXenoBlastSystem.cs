using System.Numerics;
using Content.Shared._MC.Knockback;
using Content.Shared._MC.Line;
using Content.Shared._MC.Stun;
using Content.Shared._MC.Xeno.Hive.Systems;
using Content.Shared.Damage;
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
    [Dependency] private readonly DamageableSystem _damageable = null!;

    [Dependency] private readonly MCKnockbackSystem _mcKnockback = null!;
    [Dependency] private readonly MCSharedXenoHiveSystem _mcXenoHive = null!;
    [Dependency] private readonly MCStunSystem _mcStun = null!;
    [Dependency] private readonly MCLineSystem _mcLine = null!;

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

        if (!TryUseAction(entity, args.Action))
            return;

        var coordinatesStart = _transform.GetMapCoordinates(entity);
        var coordinatesTarget = _transform.ToMapCoordinates(args.Target);

        var direction = coordinatesTarget.Position - coordinatesStart.Position;
        var distance = float.Min(direction.Length(), entity.Comp.Range);

        if (distance <= 0)
            return;

        var ray = new CollisionRay(coordinatesStart.Position, direction.Normalized(), (int) CollisionGroup.Opaque);
        var results = _physics.IntersectRayWithPredicate(
            _transform.GetMapId(entity.Owner),
            ray,
            distance,
            ent => ent == entity.Owner,
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
            new MCXenoBlastDoAfterEvent(coordinatesStart, finalTarget),
            entity.Owner)
        {
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfter);

        args.Handled = true;
    }

    private void OnDoAfter(Entity<MCXenoBlastComponent> entity, ref MCXenoBlastDoAfterEvent args)
    {
        if (args.Cancelled)
            return;

        Cast(entity, args.Start, args.Target);
    }

    public void Cast(Entity<MCXenoBlastComponent> entity, MapCoordinates startCoords, MapCoordinates targetCoords)
    {
        _audio.PlayPredicted(entity.Comp.EffectSound, _transform.ToCoordinates(targetCoords), entity);
        _mcLine.SpawnEffect(entity.Comp.RayEffectId, startCoords, targetCoords);
        ServerSpawn(entity.Comp.EffectId, targetCoords);

        var entities = _lookup.GetEntitiesInRange(targetCoords, entity.Comp.EffectRange, LookupFlags.Dynamic | LookupFlags.Approximate);
        foreach (var targetUid in entities)
        {
            if (entity.Owner == targetUid)
                continue;

            if (_mcXenoHive.FromSameHive(entity.Owner, targetUid))
                continue;

            _damageable.TryChangeDamage(targetUid, entity.Comp.Damage, origin: entity, tool: entity, armorPiercing: entity.Comp.ArmorPiercing);
            _mcStun.Slowdown(targetUid, entity.Comp.SlowdownDuration);

            var victimPos = _transform.GetMapCoordinates(targetUid);
            var direction = victimPos.Position - targetCoords.Position;
            _mcKnockback.Knockback(targetUid, direction, entity.Comp.KnockbackEntry);

            RaiseEffect(entity, targetUid);
        }
    }
}
