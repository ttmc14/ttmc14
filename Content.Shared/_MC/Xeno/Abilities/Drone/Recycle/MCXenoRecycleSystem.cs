using Content.Shared.DoAfter;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Plasma;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Shared._MC.Xeno.Abilities.Drone.Recycle;

public sealed class MCXenoRecycleSystem : MCXenoAbilitySystem
{

    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;
    [Dependency] private readonly SharedAudioSystem _audio = default!;
    [Dependency] private readonly SharedPopupSystem _popup = default!;
    [Dependency] private readonly INetManager _net = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoRecycleComponent, MCXenoRecycleActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoRecycleComponent, MCXenoRecycleDoAfterEvent>(OnXenoRecycleDoAfter);
    }

    private void OnAction(Entity<MCXenoRecycleComponent> entity, ref MCXenoRecycleActionEvent args)
    {
        if (IsXeno(args.Target))
        {
            _popup.PopupClient(Loc.GetString("recycle-no-sister"), entity, entity, PopupType.MediumCaution);
            return;
        }

        if (!IsDead(args.Target))
        {
            _popup.PopupClient(Loc.GetString("recycle-no-dead"), entity, entity, PopupType.MediumCaution);
            return;
        }

        if (!RMCActions.CanUseActionPopup(entity, args.Action, entity))
            return;

        _popup.PopupClient(Loc.GetString("recycle-start"), entity, entity, PopupType.MediumCaution);
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, entity, entity.Comp.Delay, new MCXenoRecycleDoAfterEvent(args.Action, EntityManager), entity, args.Target)
        {
            BreakOnMove = true,
            ForceVisible = true,
            CancelDuplicate = true,
        });
    }

    private void OnXenoRecycleDoAfter(Entity<MCXenoRecycleComponent> entity, ref MCXenoRecycleDoAfterEvent args)
    {
        var target = args.Target;
        if (args.Handled || args.Cancelled || target is null)
            return;

        var action = args.GetAction(EntityManager);
        if (RMCActions.TryUseAction(entity, action, entity))
            return;

        args.Handled = true;

        _audio.PlayPredicted(entity.Comp.EffectSound, entity, entity);
        _popup.PopupClient(Loc.GetString("recycle-end"), entity, entity, PopupType.MediumCaution);

        ServerQueueDel(target);
    }
}
