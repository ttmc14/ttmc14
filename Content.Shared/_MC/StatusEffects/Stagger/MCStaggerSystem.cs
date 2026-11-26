using Content.Shared._MC.StatusEffects.Stun.Components;
using Content.Shared._MC.Stun.Events;
using Content.Shared._MC.Weapon.Range.Events;
using Content.Shared._RMC14.Actions;
using Content.Shared.Actions;
using Content.Shared.Actions.Events;
using Content.Shared.Mobs.Components;
using Content.Shared.StatusEffectNew;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.StatusEffects.Stagger;

public sealed class MCStaggerSystem : EntitySystem
{
    private static readonly EntProtoId Effect = "MCStatusEffectStagger";

    [Dependency] private readonly SharedStatusEffectsSystem _effects = null!;
    [Dependency] private readonly SharedActionsSystem _actions = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MobStateComponent, BeforeStatusEffectAddedEvent>(OnBeforeAdded);

        SubscribeLocalEvent<MCStaggerComponent, StatusEffectAppliedEvent>(OnApplied);
        SubscribeLocalEvent<MCStaggerComponent, StatusEffectRemovedEvent>(OnRemoved);
        SubscribeLocalEvent<MCStaggerComponent, StatusEffectRelayedEvent<GetShooterSpreadEvent>>(OnGetSpread);
        SubscribeLocalEvent<MCStaggerComponent, StatusEffectRelayedEvent<GetShooterDamageMultiplierEvent>>(OnGetDamageMultiplier);

        SubscribeLocalEvent<MCStaggerableActionComponent, ActionAttemptEvent>(OnActionAttempt);
        SubscribeLocalEvent<MCStaggerableActionComponent, RMCActionUseAttemptEvent>(OnActionUseAttempt);

        SubscribeLocalEvent<RMCDazeableActionComponent, ActionAttemptEvent>(OnActionAttempt);
        SubscribeLocalEvent<RMCDazeableActionComponent, RMCActionUseAttemptEvent>(OnActionUseAttempt);
    }

    /// <summary>
    /// Applies stagger only when the target has no active stagger.
    /// </summary>
    public void Stagger(EntityUid target, TimeSpan duration)
    {
        if (duration <= TimeSpan.Zero || _effects.HasStatusEffect(target, Effect))
            return;

        _effects.TrySetStatusEffectDuration(target, Effect, duration);
    }

    private void OnBeforeAdded(Entity<MobStateComponent> target, ref BeforeStatusEffectAddedEvent args)
    {
        if (args.Effect == Effect && !CanStagger(target))
            args.Cancelled = true;
    }

    private bool CanStagger(EntityUid target)
    {
        var attempt = new MCStaggerAttemptEvent();
        RaiseLocalEvent(target, ref attempt);

        return !attempt.Canceled;
    }

    private void OnApplied(Entity<MCStaggerComponent> effect, ref StatusEffectAppliedEvent args)
    {
        foreach (var action in _actions.GetActions(args.Target))
        {
            if (!action.Comp.Enabled || !(HasComp<MCStaggerableActionComponent>(action) || HasComp<RMCDazeableActionComponent>(action)))
                continue;

            _actions.SetEnabled(action.AsNullable(), false);
            effect.Comp.DisabledActions.Add(action);
        }

        var ev = new MCStaggerEvent();
        RaiseLocalEvent(args.Target, ref ev);
    }

    private static void OnGetSpread(Entity<MCStaggerComponent> effect, ref StatusEffectRelayedEvent<GetShooterSpreadEvent> args) =>
        args.Args.AdditionalSpread += effect.Comp.AdditionalSpread;

    private static void OnGetDamageMultiplier(Entity<MCStaggerComponent> effect, ref StatusEffectRelayedEvent<GetShooterDamageMultiplierEvent> args) =>
        args.Args.Multiplier *= effect.Comp.DamageMultiplier;

    private void OnRemoved(Entity<MCStaggerComponent> effect, ref StatusEffectRemovedEvent args)
    {
        foreach (var action in effect.Comp.DisabledActions)
        {
            _actions.SetEnabled(action, true);
        }
    }

    private void OnActionAttempt<T>(Entity<T> action, ref ActionAttemptEvent args) where T : IComponent =>
        args.Cancelled |= _effects.HasEffectComp<MCStaggerComponent>(args.User);

    private void OnActionUseAttempt<T>(Entity<T> action, ref RMCActionUseAttemptEvent args) where T : IComponent =>
        args.Cancelled |= _effects.HasEffectComp<MCStaggerComponent>(args.User);
}
