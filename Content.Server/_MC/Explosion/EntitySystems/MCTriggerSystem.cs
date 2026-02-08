using Content.Server._MC.Bomb.Components;
using Content.Server._MC.Bomb.Systems;
using Content.Server.Explosion.Components;
using Content.Shared._MC.Bomb.Components;
using Content.Shared.Examine;
using Content.Shared.Explosion.Components;
using Content.Shared.Interaction.Events;
using Content.Shared.Popups;
using Content.Shared.Sticky;
using Content.Shared.Verbs;
using Robust.Shared.GameObjects;

namespace Content.Server.Explosion.EntitySystems;

public sealed partial class TriggerSystem
{
    [Dependency] private readonly MCBombPasswordSystem _bombPassword = default!;
    [Dependency] private readonly MCDefusableSystem _defusable = default!;

    private void InitializeOnUseMC()
    {
        SubscribeLocalEvent<OnUseTimerTriggerComponent, UseInHandEvent>(OnTimerUseMC);
    }

    private void OnTimerUseMC(EntityUid uid, OnUseTimerTriggerComponent component, UseInHandEvent args)
    {
        if (args.Handled || HasComp<AutomatedTimerComponent>(uid) || component.UseVerbInstead)
            return;

        // Check if password is required and set
        if (TryComp<MCBombPasswordComponent>(uid, out var passwordComp))
        {
            if (!_bombPassword.CanActivate((uid, passwordComp)))
            {
                _popupSystem.PopupEntity(Loc.GetString("bomb-password-not-set"), uid, args.User, PopupType.MediumCaution);
                args.Handled = true;
                return;
            }
        }

        // If this is a defusable bomb, use the proper defusable system to start countdown
        // This ensures proper anchoring, bolting, and warnings
        if (TryComp<MCDefusableComponent>(uid, out var defusableComp))
        {
            _defusable.TryStartCountdown(uid, args.User, defusableComp);
            args.Handled = true;
            return;
        }

        // For non-defusable items, use the standard timer activation
        if (component.DoPopup)
            _popupSystem.PopupEntity(Loc.GetString("trigger-activated", ("device", uid)), args.User, args.User);

        StartTimer((uid, component), args.User);

        args.Handled = true;
    }
}
