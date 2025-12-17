using Content.Shared._MC.Xeno.Abilities.ResinJelly.Components;
using Content.Shared._MC.Xeno.Abilities.ResinJelly.Events;
using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Atmos;
using Content.Shared._RMC14.Aura;
using Content.Shared._RMC14.Emote;
using Content.Shared._RMC14.Hands;
using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Components;
using Robust.Shared.Network;

namespace Content.Shared._MC.Xeno.Abilities.ResinJelly;

public sealed class MCXenoResinJellySystem : MCXenoAbilitySystem
{
    [Dependency] private readonly INetManager _net = null!;

    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedHandsSystem _hands = null!;

    [Dependency] private readonly SharedAuraSystem _rmcAura = null!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = null!;
    [Dependency] private readonly SharedRMCEmoteSystem _rmcEmote = null!;
    [Dependency] private readonly SharedRMCFlammableSystem _rmcFlammable = null!;
    [Dependency] private readonly SharedXenoHiveSystem _rmcXenoHive = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoCreateResinJellyComponent, MCXenoCreateResinJellyActionEvent>(OnCreateActionEvent);

        SubscribeLocalEvent<MCXenoResinJellyComponent, AfterInteractEvent>(OnAfterInteract);
        SubscribeLocalEvent<MCXenoResinJellyComponent, ActivateInWorldEvent>(OnPickupAttempt, after: [ typeof(RMCHandsSystem) ]);
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
        _rmcXenoHive.SetSameHive(entity.Owner, instance);
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

    private void OnPickupAttempt(Entity<MCXenoResinJellyComponent> entity, ref ActivateInWorldEvent args)
    {
        TryConsume(entity, args.User, args.User);
    }

    private void OnAfterInteractDoAfter(Entity<MCXenoResinJellyComponent> entity, ref MCXenoResinJellyConsumeDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;

        if (args.Target is not { } target)
            return;

        _rmcAura.GiveAura(target, entity.Comp.AuraColor, entity.Comp.Duration);
        _rmcEmote.TryEmoteWithChat(target, entity.Comp.Emote);
        _rmcFlammable.Extinguish(target);

        EnsureComp<MCXenoResinJellyFireproofComponent>(target);

        if (_net.IsClient)
            return;

        RemCompDeferredDelayed<MCXenoResinJellyFireproofComponent>(target, entity.Comp.Duration);
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

        if (!_rmcXenoHive.FromSameHive(entity.Owner, user))
            return false;

        if (!_rmcXenoHive.FromSameHive(user, target))
            return false;

        var applyDuration = user == target ? entity.Comp.DelaySelf : entity.Comp.DelayOther;

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
