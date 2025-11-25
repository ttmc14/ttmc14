using Content.Shared._RMC14.Weapons.Ranged.IFF;
using Content.Shared.Actions;
using Content.Shared.Hands;
using Content.Shared.Movement.Systems;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;

namespace Content.Shared._MC.Weapon.Aimed;

public sealed class MCAimedShootSystem : EntitySystem
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
        SubscribeLocalEvent<MCAimedShootComponent, AmmoShotEvent>(OnAmmoShot);
        SubscribeLocalEvent<MCAimedShootComponent, GunRefreshModifiersEvent>(OnRefreshModifiers);
        SubscribeLocalEvent<MCAimedShootComponent, GotEquippedHandEvent>(OnGotEquippedHand);
        SubscribeLocalEvent<MCAimedShootComponent, GotUnequippedHandEvent>(OnGotUnequippedHand);
        SubscribeLocalEvent<MCAimedShootComponent, HeldRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnRefreshMovementSpeedModifiers);
    }

    private void OnGetItemActions(Entity<MCAimedShootComponent> entity, ref GetItemActionsEvent args)
    {
        if (!args.InHands)
            return;

        args.AddAction(ref entity.Comp.Action, entity.Comp.ActionId);
        Dirty(entity);
    }

    private void OnToggleAction(Entity<MCAimedShootComponent> entity, ref MCAimedShootActionEvent args)
    {
        if (args.Handled)
            return;

        args.Handled = true;

        entity.Comp.Active = !entity.Comp.Active;
        Dirty(entity);

        _actions.SetToggled(entity.Comp.Action, entity.Comp.Active);

        _gun.RefreshModifiers(entity.Owner);
        _movementSpeed.RefreshMovementSpeedModifiers(args.Performer);
    }

    private void OnAmmoShot(Entity<MCAimedShootComponent> entity, ref AmmoShotEvent args)
    {
        if (!entity.Comp.Active)
            return;

        _gunIFF.GiveAmmoIFF(entity, ref args, false, true);
    }

    private void OnGotEquippedHand(Entity<MCAimedShootComponent> entity, ref GotEquippedHandEvent args)
    {
        _gun.RefreshModifiers(entity.Owner);
        _movementSpeed.RefreshMovementSpeedModifiers(args.User);
    }

    private void OnGotUnequippedHand(Entity<MCAimedShootComponent> entity, ref GotUnequippedHandEvent args)
    {
        entity.Comp.Active = false;
        Dirty(entity);

        _gun.RefreshModifiers(entity.Owner);
        _movementSpeed.RefreshMovementSpeedModifiers(args.User);
    }

    private void OnRefreshMovementSpeedModifiers(Entity<MCAimedShootComponent> entity, ref HeldRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        if (!entity.Comp.Active)
            return;

        args.Args.ModifySpeed(entity.Comp.AimSpeedModifier, entity.Comp.AimSpeedModifier);
    }

    private void OnRefreshModifiers(Entity<MCAimedShootComponent> entity, ref GunRefreshModifiersEvent args)
    {
        if (!entity.Comp.Active)
            return;

        args.FireRate *= entity.Comp.AimFireModifier;
    }
}
