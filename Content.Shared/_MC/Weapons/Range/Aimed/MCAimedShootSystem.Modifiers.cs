using Content.Shared.Hands;
using Content.Shared.Movement.Systems;
using Content.Shared.Weapons.Ranged.Events;

namespace Content.Shared._MC.Weapons.Range.Aimed;

public sealed partial class MCAimedShootSystem
{
    private void RefreshModifiers(Entity<MCAimedShootComponent> entity, EntityUid userUid)
    {
        _gun.RefreshModifiers(entity.Owner);
        _movementSpeed.RefreshMovementSpeedModifiers(userUid);
    }

    private void OnAmmoShotModifiers(Entity<MCAimedShootComponent> entity, ref AmmoShotEvent args)
    {
        if (entity.Comp.Active)
            _gunIFF.GiveAmmoIFF(entity, ref args, false, true);
    }

    private static void OnRefreshMovementSpeedModifiers(Entity<MCAimedShootComponent> entity, ref HeldRelayedEvent<RefreshMovementSpeedModifiersEvent> args)
    {
        if (entity.Comp.Active)
            args.Args.ModifySpeed(entity.Comp.AimSpeedModifier, entity.Comp.AimSpeedModifier);
    }

    private static void OnRefreshModifiers(Entity<MCAimedShootComponent> entity, ref GunRefreshModifiersEvent args)
    {
        if (entity.Comp.Active)
            args.FireRate *= entity.Comp.AimFireModifier;
    }
}
