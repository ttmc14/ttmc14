using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.CameraShake;
using Content.Shared._RMC14.Pulling;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Shared._MC.Xeno.Abilities.Stomp;

public sealed class MCXenoStompSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly EntityLookupSystem _lookup = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly SharedXenoHiveSystem _xenoHive = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly RMCPullingSystem _rmcPulling = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly RMCCameraShakeSystem _rmcCameraShake = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoStompComponent, MCXenoStompActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoStompComponent> entity, ref MCXenoStompActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
            return;

        args.Handled = true;

        var coordinates = Transform(entity).Coordinates;
        foreach (var target in _lookup.GetEntitiesInRange<MobStateComponent>(coordinates, entity.Comp.Distance))
        {
            if (!HasComp<MobStateComponent>(target) || _mobState.IsDead(target))
                continue;

            if (_xenoHive.FromSameHive(entity.Owner, target.Owner))
                continue;

            var targetCoordinates = Transform(target).Coordinates;
            var delta = (targetCoordinates - coordinates).Position;
            var distance = delta.Length();
            var damage = entity.Comp.Damage / Math.Max(1, distance + 1);

            if (distance <= 1.1f)
            {
                _damageable.TryChangeDamage(target, damage, origin: entity, tool: entity);
                _stun.TryParalyze(target, entity.Comp.Paralyze, true);
                _rmcCameraShake.ShakeCamera(target, 3, 3);
                continue;
            }

            _damageable.TryChangeDamage(target, damage, origin: entity, tool: entity);

            _rmcPulling.TryStopAllPullsFromAndOn(target);
            _throwing.TryThrow(target, delta.Normalized() * entity.Comp.ThrowDistance, entity.Comp.ThrowSpeed);

            _rmcCameraShake.ShakeCamera(target, 2, 2);
            _stun.TryParalyze(target, entity.Comp.ThrowParalyze, true);
        }

        _audio.PlayPredicted(entity.Comp.Sound, entity, entity);

        if (_net.IsClient)
            return;

        SpawnAttachedTo(entity.Comp.Effect, entity.Owner.ToCoordinates());
    }
}
