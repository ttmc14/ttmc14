using Content.Server._MC.Bomb.Components;
using Content.Server._MC.Bomb.Systems;
using Content.Server.Explosion.Components;
using Content.Server.Popups;
using Content.Shared._MC.Bomb.Components;
using Content.Shared._MC.Explosion.Components;
using Content.Shared.Examine;
using Content.Shared.Explosion.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Sticky;
using Content.Shared.Verbs;
using Robust.Shared.GameObjects;
using Robust.Shared.Random;

namespace Content.Server.Explosion.EntitySystems;

public sealed partial class MCTriggerSystem
{
    private void InitializeOnUse()
    {
        SubscribeLocalEvent<MCOnUseTimerTriggerComponent, UseInHandEvent>(OnTimerUse);
        SubscribeLocalEvent<MCOnUseTimerTriggerComponent, ExaminedEvent>(OnExamined);
        SubscribeLocalEvent<MCOnUseTimerTriggerComponent, GetVerbsEvent<AlternativeVerb>>(OnGetAltVerbs);
        SubscribeLocalEvent<MCOnUseTimerTriggerComponent, EntityStuckEvent>(OnStuck);
    }

    private void OnStuck(EntityUid uid, MCOnUseTimerTriggerComponent component, ref EntityStuckEvent args)
    {
        if (!component.StartOnStick)
            return;

        StartTimer((uid, component), args.User);
    }

    private void OnExamined(EntityUid uid, MCOnUseTimerTriggerComponent component, ExaminedEvent args)
    {
        if (args.IsInDetailsRange && component.Examinable)
            args.PushText(Loc.GetString("examine-trigger-timer", ("time", component.Delay)));
    }

    private void OnGetAltVerbs(EntityUid uid, MCOnUseTimerTriggerComponent component, GetVerbsEvent<AlternativeVerb> args)
    {
        if (!args.CanInteract || !args.CanAccess || args.Hands == null)
            return;

        if (component.UseVerbInstead)
        {
            args.Verbs.Add(new AlternativeVerb()
            {
                Text = Loc.GetString("verb-start-detonation"),
                Act = () => StartTimer((uid, component), args.User),
                Priority = 2
            });
        }

        if (component.AllowToggleStartOnStick)
        {
            args.Verbs.Add(new AlternativeVerb()
            {
                Text = Loc.GetString("verb-toggle-start-on-stick"),
                Act = () => ToggleStartOnStick(uid, args.User, component)
            });
        }

        if (component.DelayOptions == null || component.DelayOptions.Count == 1)
            return;

        args.Verbs.Add(new AlternativeVerb()
        {
            Category = TimerOptions,
            Text = Loc.GetString("verb-trigger-timer-cycle"),
            Act = () => CycleDelay(component, args.User),
            Priority = 1
        });

        foreach (var option in component.DelayOptions)
        {
            if (MathHelper.CloseTo(option, component.Delay))
            {
                args.Verbs.Add(new AlternativeVerb()
                {
                    Category = TimerOptions,
                    Text = Loc.GetString("verb-trigger-timer-set-current", ("time", option)),
                    Disabled = true,
                    Priority = (int) (-100 * option)
                });
                continue;
            }

            args.Verbs.Add(new AlternativeVerb()
            {
                Category = TimerOptions,
                Text = Loc.GetString("verb-trigger-timer-set", ("time", option)),
                Priority = (int) (-100 * option),

                Act = () =>
                {
                    component.Delay = option;
                    _popupSystem.PopupEntity(Loc.GetString("popup-trigger-timer-set", ("time", option)), args.User, args.User);
                },
            });
        }
    }

    private void CycleDelay(MCOnUseTimerTriggerComponent component, EntityUid user)
    {
        if (component.DelayOptions == null || component.DelayOptions.Count == 1)
            return;

        component.DelayOptions.Sort();

        if (component.DelayOptions[^1] <= component.Delay)
        {
            component.Delay = component.DelayOptions[0];
            _popupSystem.PopupEntity(Loc.GetString("popup-trigger-timer-set", ("time", component.Delay)), user, user);
            return;
        }

        foreach (var option in component.DelayOptions)
        {
            if (option > component.Delay)
            {
                component.Delay = option;
                _popupSystem.PopupEntity(Loc.GetString("popup-trigger-timer-set", ("time", option)), user, user);
                return;
            }
        }
    }

    private void ToggleStartOnStick(EntityUid grenade, EntityUid user, MCOnUseTimerTriggerComponent comp)
    {
        if (comp.StartOnStick)
        {
            comp.StartOnStick = false;
            _popupSystem.PopupEntity(Loc.GetString("popup-start-on-stick-off"), grenade, user);
        }
        else
        {
            comp.StartOnStick = true;
            _popupSystem.PopupEntity(Loc.GetString("popup-start-on-stick-on"), grenade, user);
        }
    }

    private void OnTimerUse(EntityUid uid, MCOnUseTimerTriggerComponent component, UseInHandEvent args)
    {
        if (args.Handled || HasComp<AutomatedTimerComponent>(uid) || component.UseVerbInstead)
            return;

        if (TryComp<MCBombPasswordComponent>(uid, out var passwordComp))
        {
            if (!_bombPassword.CanActivate((uid, passwordComp)))
            {
                _popupSystem.PopupEntity(Loc.GetString("bomb-password-not-set"), uid, args.User, PopupType.MediumCaution);
                args.Handled = true;
                return;
            }
        }

        if (TryComp<MCDefusableComponent>(uid, out var defusableComp))
        {
            _defusable.TryStartCountdown(uid, args.User, defusableComp);
            args.Handled = true;
            return;
        }

        if (component.DoPopup)
            _popupSystem.PopupEntity(Loc.GetString("trigger-activated", ("device", uid)), args.User, args.User);

        StartTimer((uid, component), args.User);

        args.Handled = true;
    }

    public static VerbCategory TimerOptions = new("verb-categories-timer", "/Textures/Interface/VerbIcons/clock.svg.192dpi.png");


    private void StartTimer(Entity<MCOnUseTimerTriggerComponent?> ent, EntityUid? user)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        var comp = ent.Comp!;
        _triggerSystem.HandleTimerTrigger(ent.Owner, user, comp.Delay, comp.BeepInterval, comp.InitialBeepDelay, comp.BeepSound);
    }
}
