using Content.Shared._RMC14.Weapons.Ranged.IFF;
using Content.Shared.Actions;
using Content.Shared.Hands;
using Content.Shared.Movement.Systems;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared._MC.Weapons.Range.Aimed;

public sealed partial class MCAimedShootSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = null!;
    [Dependency] private readonly GunIFFSystem _gunIFF = null!;
    [Dependency] private readonly SharedGunSystem _gun = null!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCAimedShootComponent, GetItemActionsEvent>(OnGetItemActions);
        SubscribeLocalEvent<MCAimedShootComponent, MCAimedShootActionEvent>(OnToggleAction);

        SubscribeLocalEvent<MCAimedShootComponent, AmmoShotEvent>(OnAmmoShotModifiers);
        SubscribeLocalEvent<MCAimedShootComponent, GotEquippedHandEvent>(OnGotEquippedHand);
        SubscribeLocalEvent<MCAimedShootComponent, GotUnequippedHandEvent>(OnGotUnequippedHand);

        SubscribeLocalEvent<MCAimedShootComponent, GunRefreshModifiersEvent>(OnRefreshModifiers);
        SubscribeLocalEvent<MCAimedShootComponent, HeldRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnRefreshMovementSpeedModifiers);
    }

    private void OnGetItemActions(Entity<MCAimedShootComponent> entity, ref GetItemActionsEvent args)
    {
        if (!args.InHands)
            return;

        args.AddAction(ref entity.Comp.Action, entity.Comp.ActionId);
        _actions.SetToggled(entity.Comp.Action, entity.Comp.Active);
    }

    private void OnToggleAction(Entity<MCAimedShootComponent> entity, ref MCAimedShootActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        entity.Comp.Active = !entity.Comp.Active;
        _actions.SetToggled(entity.Comp.Action, entity.Comp.Active);

        RefreshModifiers(entity, args.Performer);
    }

    private void OnGotEquippedHand(Entity<MCAimedShootComponent> entity, ref GotEquippedHandEvent args)
    {
        RefreshModifiers(entity, args.User);
    }

    private void OnGotUnequippedHand(Entity<MCAimedShootComponent> entity, ref GotUnequippedHandEvent args)
    {
        RefreshModifiers(entity, args.User);
    }
}
