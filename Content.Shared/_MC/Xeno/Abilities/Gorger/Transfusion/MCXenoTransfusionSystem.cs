using System.Linq;
using Content.Shared._MC.Xeno.Abilities.Gorger.Transfusion.Components;
using Content.Shared._MC.Xeno.Abilities.Gorger.Transfusion.Events;
using Content.Shared._MC.Xeno.Heal;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;
using Content.Shared.Mobs.Systems;
using Content.Shared.Stunnable;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Transfusion;

public sealed class MCXenoTransfusionSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedInteractionSystem _interaction = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    [Dependency] private readonly MCXenoHealSystem _heal = null!;
    [Dependency] private readonly MCXenoOverhealSystem _overheal = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoTransfusionComponent, MCXenoTransfusionActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoTransfusionComponent, MCXenoTransfusionDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<MCXenoTransfusionComponent, DoAfterAttemptEvent<MCXenoTransfusionDoAfterEvent>>(OnAttempt);
    }

    private void OnAction(Entity<MCXenoTransfusionComponent> ent, ref MCXenoTransfusionActionEvent args)
    {
        if (args.Handled || !IsLivingHiveMember(ent, args.Target) || !CanUseAction(ent, args.Action))
            return;

        if (!TryComp<DoAfterComponent>(ent, out var pending) ||
            pending.DoAfters.Values.Any(channel => !channel.Cancelled && !channel.Completed))
            return;

        var ev = new MCXenoTransfusionDoAfterEvent(args.Action, EntityManager);
        var doAfter = new DoAfterArgs(EntityManager, ent, ent.Comp.Delay, ev, ent, args.Target)
        {
            // OnAttempt checks the live beam endpoints instead of lag-compensated hitboxes.
            RangeCheck = false,
            AttemptFrequency = AttemptFrequency.EveryTick,
            DuplicateCondition = DuplicateConditions.SameEvent,
            CancelDuplicate = false,
            RequireCanInteract = false,
        };

        // EveryTick also checks the recipient synchronously during startup.
        ent.Comp.PreviousHealth = _heal.GetHealth(args.Target);
        ent.Comp.WasCritical = _mobState.IsCritical(args.Target);

        if (!_doAfter.TryStartDoAfter(doAfter, out var id, pending) || !_doAfter.IsRunning(id, pending))
            return;

        ent.Comp.Recipient = args.Target;
        Dirty(ent);
    }

    private void OnAttempt(Entity<MCXenoTransfusionComponent> ent, ref DoAfterAttemptEvent<MCXenoTransfusionDoAfterEvent> args)
    {
        if (args.DoAfter.Args.Target is not { } target || !IsLivingHiveMember(ent, target))
        {
            args.Cancel();
            return;
        }

        if (!_interaction.InRangeUnobstructed(
                _transform.GetMapCoordinates(ent.Owner),
                _transform.GetMapCoordinates(target),
                ent.Comp.Range,
                predicate: uid => uid == ent.Owner || uid == target)
            )
        {
            args.Cancel();
            return;
        }

        var health = _heal.GetHealth(target);
        if (!ent.Comp.WasCritical
            && _mobState.IsCritical(target)
            || health < ent.Comp.PreviousHealth
            && !HasComp<KnockedDownComponent>(target)
            && !_mobState.IsIncapacitated(target))
        {
            args.Cancel();
        }

        ent.Comp.PreviousHealth = health;
        ent.Comp.WasCritical = _mobState.IsCritical(target);
    }

    private void OnDoAfter(Entity<MCXenoTransfusionComponent> ent, ref MCXenoTransfusionDoAfterEvent args)
    {
        ent.Comp.Recipient = null;
        Dirty(ent);

        if (args.Cancelled || !_mobState.IsAlive(ent) || args.Target is not { } target || !IsLivingHiveMember(ent, target))
            return;

        var action = args.GetAction(EntityManager);

        if (!RMCActions.TryUseAction(ent, action, ent))
            return;

        args.Handled = true;

        _audio.PlayPredicted(ent.Comp.TransferSound, target, ent);
        _overheal.Restore(target, _heal.GetHealthAlive(target) * ent.Comp.HealFraction, effect: "MCEffectBloodHealing");

        ActionStartUseDelay<MCXenoTransfusionActionEvent>(ent, action);
    }
}
