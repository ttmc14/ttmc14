using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.CameraShake;
using Content.Shared._RMC14.Pulling;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Mobs.Components;
using Content.Shared.Stunnable;
using Content.Shared.Throwing;
using Robust.Shared.Audio;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.PsychicFling;

public sealed class MCXenoPsychicFlingSystem : EntitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly SharedXenoHiveSystem _xenoHive = default!;
    [Dependency] private readonly SharedStunSystem _stun = default!;
    [Dependency] private readonly RMCCameraShakeSystem _camera = default!;
    [Dependency] private readonly RMCPullingSystem _rmcPulling = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoPsychicFlingComponent, MCXenoPsychicFlingActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoPsychicFlingComponent> entity, ref MCXenoPsychicFlingActionEvent args)
    {
        if (args.Handled)
            return;

        if (_xenoHive.FromSameHive(entity.Owner, args.Target))
            return;

        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
            return;

        args.Handled = true;

        _audio.PlayPredicted(new SoundPathSpecifier("/Audio/_MC/Effects/magic.ogg"), entity, entity);
        _audio.PlayPredicted(new SoundPathSpecifier("/Audio/_MC/Effects/alien_claw_block.ogg"), entity, entity);

        var distance = entity.Comp.ItemDistance;
        if (HasComp<MobStateComponent>(args.Target))
        {
            distance = entity.Comp.MobDistance;

            _stun.TryStun(args.Target, entity.Comp.StunDuration, true);
            _stun.TryParalyze(args.Target, entity.Comp.ParalyzeDuration, true);
            _camera.ShakeCamera(args.Target, 2, 1);
        }

        var originCoordinates = _transform.GetMapCoordinates(entity);
        var targetCoordinates = _transform.GetMapCoordinates(args.Target);
        var delta = (targetCoordinates.Position - originCoordinates.Position).Normalized() * distance;

        _rmcPulling.TryStopAllPullsFromAndOn(args.Target);
        _throwing.TryThrow(args.Target, delta, entity.Comp.Speed);
    }
}
