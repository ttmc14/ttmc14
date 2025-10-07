using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Atmos;
using Content.Shared._RMC14.Aura;
using Content.Shared._RMC14.Emote;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Content.Shared.Popups;
using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.ResinJelly;

public sealed class MCXenoResinJellySystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;

    [Dependency] private readonly SharedRMCEmoteSystem _emote = default!;
    [Dependency] private readonly SharedAuraSystem _aura = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedHandsSystem _hands = default!;
    [Dependency] private readonly SharedXenoHiveSystem _xenoHive = default!;

    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoCreateResinJellyComponent, MCXenoCreateResinJellyActionEvent>(OnCreateActionEvent);

        SubscribeLocalEvent<MCXenoResinJellyComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<MCXenoResinJellyComponent, MCXenoResinJellyConsumeDoAfterEvent>(OnAfterInteractDoAfter);

        SubscribeLocalEvent<MCXenoResinJellyFireproofComponent, RMCIgniteAttemptEvent>(OnFireproofIgniteAttempt);
    }

    private void OnCreateActionEvent(Entity<MCXenoCreateResinJellyComponent> entity, ref MCXenoCreateResinJellyActionEvent args)
    {
        if (args.Handled)
            return;

        if (_hands.GetActiveHand(entity.Owner) is not { } handId)
            return;

        if (_hands.GetHeldItem(entity.Owner, handId) is not null)
            return;

        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
            return;

        args.Handled = true;

        if (_net.IsClient)
            return;

        var instance = Spawn(entity.Comp.ProtoId);
        _xenoHive.SetSameHive(entity.Owner, instance);
        _hands.TryPickup(entity, instance, handId, false, false);
    }

    private void OnAfterInteract(Entity<MCXenoResinJellyComponent> entity, ref AfterInteractEvent args)
    {
        if (!args.CanReach || args.Target is not { } target)
            return;

        if (!HasComp<MobStateComponent>(target))
            return;

        TryConsume(entity, args.User, target);
    }

    private void OnAfterInteractDoAfter(Entity<MCXenoResinJellyComponent> entity, ref MCXenoResinJellyConsumeDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;

        if (args.Target is not { } target)
            return;

        _aura.GiveAura(target, entity.Comp.AuraColor, entity.Comp.Duration);
        _emote.TryEmoteWithChat(target, entity.Comp.Emote);

        EnsureComp<MCXenoResinJellyFireproofComponent>(target);

        if (_net.IsClient)
            return;

        Timer.Spawn(entity.Comp.Duration,
            () =>
            {
                RemCompDeferred<MCXenoResinJellyFireproofComponent>(target);
            }
        );

        Del(entity);
    }

    private void OnFireproofIgniteAttempt(Entity<MCXenoResinJellyFireproofComponent> entity, ref RMCIgniteAttemptEvent args)
    {
        args.Cancel();
    }

    private bool TryConsume(Entity<MCXenoResinJellyComponent> entity, EntityUid user, EntityUid target)
    {
        if (!HasComp<XenoComponent>(user))
            return false;

        if (!HasComp<XenoComponent>(target))
            return false;

        if (HasComp<MCXenoResinJellyFireproofComponent>(target))
            return false;

        if (!_xenoHive.FromSameHive(entity.Owner, user))
            return false;

        if (!_xenoHive.FromSameHive(user, target))
            return false;

        var applyDuration = user == target ? entity.Comp.ApplySelfDuration : entity.Comp.ApplyOtherDuration;

        var ev = new MCXenoResinJellyConsumeDoAfterEvent();
        var doAfter = new DoAfterArgs(EntityManager, user, applyDuration, ev, entity, target)
        {
            NeedHand = true,
            BreakOnMove = true,
            RequireCanInteract = true,
        };

        return _doAfter.TryStartDoAfter(doAfter);
    }
}
