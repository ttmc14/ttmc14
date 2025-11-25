using System.Numerics;
using Content.Shared._MC.Flammable;
using Content.Shared._MC.Utilities.Math;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.CameraShake;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Item;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Physics.Systems;

namespace Content.Shared._MC.Xeno.Abilities.UnrelentingForce;

public sealed class MCXenoUnrelentingForceSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly EntityLookupSystem _entityLookup = default!;
    [Dependency] private readonly RMCCameraShakeSystem _rmcCameraShake = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly MCSharedFlammableSystem _mcFlammable = default!;
    [Dependency] private readonly SharedXenoHiveSystem _xenoHive = default!;
    [Dependency] private readonly SharedPhysicsSystem _physics = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoUnrelentingForceComponent, MCXenoUnrelentingForceActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoUnrelentingForceComponent> entity, ref MCXenoUnrelentingForceActionEvent args)
    {
        if (args.Handled || !_rmcActions.TryUseAction(entity, args.Action, entity))
            return;

        args.Handled = true;

        var origin = _transform.GetMapCoordinates(entity);

        var target = _transform.ToMapCoordinates(args.Target);

        var direction = (target.Position - origin.Position).Normalized();
        var cardinalDirection = direction.CardinalDirection();

        const int directionMultiplier = 2;
        const int radius = 1; // for square not circle
        var corner = new Vector2i(-1, 1);

        /* This is where the fucking geometry comes forward, I'll leave a drawing of how it works
         * x - target, c - center, s - start, p - entity
         *
         * left {-1, 0}
         * x | x | x |
         * x | c | s | p
         * x | x | x |
         *
         * up {0, 1}
         * x | x | x
         * x | c | x
         * x | s | x
         *   | p |
         *
         * to get the center we just need to
         * duplicate the direction and add the starting position
         **/

        var center = origin.Position + cardinalDirection * directionMultiplier;
        var aabb = new Box2(center + corner * radius, center - corner * radius);

        foreach (var uid in _entityLookup.GetEntitiesIntersecting(origin.MapId, aabb))
        {
            ApplyEffect(uid);
        }

        foreach (var uid in _entityLookup.GetEntitiesIntersecting(origin.MapId, new Box2(origin.Position + corner / 2, origin.Position - corner / 2)))
        {
            ApplyEffect(uid);
        }

        _audio.PlayPredicted(new SoundCollectionSpecifier("XenoRoar"), entity, entity);
        _audio.PlayPredicted(new SoundPathSpecifier("/Audio/_MC/Effects/alien_claw_block.ogg"), entity, entity);

        return;
        void ApplyEffect(EntityUid uid)
        {
            if (entity.Owner == uid)
                return;

            _mcFlammable.AdjustFireStacks(uid, -10);

            if (!HasComp<MobStateComponent>(uid) && !HasComp<ItemComponent>(uid))
                return;

            if (Transform(uid).Anchored)
                return;

            if (_mobState.IsDead(uid))
                return;

            if (_xenoHive.FromSameHive(uid, entity.Owner))
                return;

            _stun.TryParalyze(uid, TimeSpan.FromSeconds(2), true);
            _rmcCameraShake.ShakeCamera(uid, 2, 1);
            _physics.SetLinearVelocity(uid, Vector2.Zero);
            _throwing.TryThrow(uid, cardinalDirection * 6, baseThrowSpeed: entity.Comp.ThrowSpeed);
        }
    }
}
