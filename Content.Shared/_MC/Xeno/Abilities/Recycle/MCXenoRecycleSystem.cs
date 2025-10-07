using Content.Shared.DoAfter;
using Content.Shared.Mobs.Systems;
using Content.Shared.Popups;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Plasma;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Network;

namespace Content.Shared._MC.Xeno.Abilities.Recycle;

public sealed class MCXenoRecycleSystem : EntitySystem
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

    private void OnAction(Entity<MCXenoRecycleComponent> xeno, ref MCXenoRecycleActionEvent args)
    {
        var target = args.Target;

        if (!HasComp<XenoComponent>(target))
        {
            _popup.PopupClient(Loc.GetString("recycle-no-sister"), xeno, xeno, PopupType.MediumCaution);
            return;
        }

        if (!_mobState.IsDead(target))
        {
            _popup.PopupClient(Loc.GetString("recycle-no-dead"), xeno, xeno, PopupType.MediumCaution);
            return;
        }

        if (!_xenoPlasma.HasPlasmaPopup(xeno.Owner, xeno.Comp.PlasmaCost))
            return;

        var doAfter = new DoAfterArgs(EntityManager, xeno, xeno.Comp.RecycleDelay, new MCXenoRecycleDoAfterEvent(), xeno, target)
        {
            BreakOnMove = true,
            BreakOnDamage = false,
            ForceVisible = true,
            CancelDuplicate = true,
            DuplicateCondition = DuplicateConditions.SameEvent
        };

        _popup.PopupClient(Loc.GetString("recycle-start"), xeno, xeno, PopupType.MediumCaution);

        if (_doAfter.TryStartDoAfter(doAfter))
            args.Handled = true;
    }

    private void OnXenoRecycleDoAfter(Entity<MCXenoRecycleComponent> xeno, ref MCXenoRecycleDoAfterEvent args)
    {
        var target = args.Target;

        if (args.Handled || args.Cancelled || target == null)
            return;

        if (!_xenoPlasma.TryRemovePlasmaPopup(xeno.Owner, xeno.Comp.PlasmaCost))
            return;

        args.Handled = true;
        if (_net.IsServer)
        {
            QueueDel(target);
            _audio.PlayPvs(xeno.Comp.Sound, xeno);
            _popup.PopupClient(Loc.GetString("recycle-end"), xeno, xeno, PopupType.MediumCaution);
        }
    }
}
