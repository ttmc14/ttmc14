using Content.Shared._MC.Jittering;
using Content.Shared._MC.Xeno.Abilities.Drone.Recycle.Events;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Drone.Recycle;

public sealed class MCXenoRecycleSystem : MCXenoAbilitySystem
{
    private static readonly LocId LocIdProcessStart = "mc-xeno-recycle-process-start";
    private static readonly LocId LocIdProcessEnd = "mc-xeno-recycle-process-end";
    private static readonly LocId LocIdTargetNotXeno = "mc-xeno-recycle-not-xeno";
    private static readonly LocId LocIdTargetNotDead = "mc-xeno-recycle-not-dead";

    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly SharedBodySystem _body = null!;

    [Dependency] private readonly MCJitteringSystem _jittering = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoRecycleComponent, MCXenoRecycleActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoRecycleComponent, MCXenoRecycleDoAfterEvent>(OnActionDoAfter);
    }

    private void OnAction(Entity<MCXenoRecycleComponent> entity, ref MCXenoRecycleActionEvent args)
    {
        var target = args.Target;

        if (!IsXeno(target))
        {
            _popup.PopupClient(Loc.GetString(LocIdTargetNotXeno), entity, entity, PopupType.MediumCaution);
            return;
        }

        if (!IsDead(target))
        {
            _popup.PopupClient(Loc.GetString(LocIdTargetNotDead), entity, entity, PopupType.MediumCaution);
            return;
        }

        if (!RMCActions.CanUseActionPopup(entity, args.Action, entity))
            return;

        _jittering.DoJitter(target, entity.Comp.EffectJitteringTarget);
        _audio.PlayPredicted(entity.Comp.EffectSoundProcessStart, entity, entity);
        _popup.PopupClient(Loc.GetString(LocIdProcessStart), entity, entity, PopupType.MediumCaution);

        var ev = new MCXenoRecycleDoAfterEvent(args.Action, EntityManager);
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, entity, entity.Comp.Delay, ev, entity, target: args.Target)
        {
            BreakOnMove = true,
            ForceVisible = true,
            CancelDuplicate = true,
        });
    }

    private void OnActionDoAfter(Entity<MCXenoRecycleComponent> entity, ref MCXenoRecycleDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Target is not { } target)
            return;

        var action = args.GetAction(EntityManager);
        if (!RMCActions.TryUseAction(entity, action, entity))
            return;

        args.Handled = true;

        _audio.PlayPredicted(entity.Comp.EffectSoundProcessEnd, entity, entity);
        _popup.PopupClient(Loc.GetString(LocIdProcessEnd), entity, entity, PopupType.MediumCaution);
        _body.GibBody(target);
    }
}
