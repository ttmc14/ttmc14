using Content.Shared._MC.Weapon.Events;
using Content.Shared.DoAfter;
using Content.Shared.Weapons.Ranged;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Map;
using Robust.Shared.Network;

namespace Content.Shared._MC.Weapon;

public sealed class MCWeaponShootSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedGunSystem _gun = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCWeaponFireDelayComponent, ShotAttemptedEvent>(OnAttemptShoot);
        SubscribeLocalEvent<MCWeaponFireDelayComponent, MCWeaponFireDelayDoAfter>(OnAttemptShootDoAfter);
    }

    private void OnAttemptShoot(Entity<MCWeaponFireDelayComponent> entity, ref ShotAttemptedEvent args)
    {
        if (args.Cancelled)
            return;

        // It's was stoping shoot, but not here. Need or popups, sounds etc
        if (GetAmmoCount(args.Used) == 0)
            return;

        if (_doAfter.IsRunning(entity.Comp.DoAfterId))
        {
            args.Cancel();
            return;
        }

        // TODO: cancel shoot with new attempt, but with delay for exclude immediately canceling
        var ev = new MCWeaponFireDelayDoAfter();
        var doAfter = new DoAfterArgs(EntityManager, args.User, entity.Comp.Delay, ev, args.Used, used: args.Used)
        {
            BreakOnMove = false,
        };

        if (!_doAfter.TryStartDoAfter(doAfter, out var doAfterId))
            return;

        entity.Comp.DoAfterId = doAfterId;

        args.Cancel();
    }

    private void OnAttemptShootDoAfter(Entity<MCWeaponFireDelayComponent> entity, ref MCWeaponFireDelayDoAfter args)
    {
        // Clean up do after state
        entity.Comp.DoAfterId = null;

        if (args.Handled || args.Cancelled)
            return;

        if (!TryComp<GunComponent>(args.Used, out var gunComponent))
            return;

        if (gunComponent.ShootCoordinates is not { } toCoordinates)
            return;

        args.Handled = true;

        var fromCoordinates = Transform(args.User).Coordinates;
        var ammo = TakeAmmo(entity, args.User, toCoordinates);

        _gun.Shoot(args.Used.Value, gunComponent, ammo, fromCoordinates, toCoordinates, out _, args.User, throwItems: false);
    }

    private int GetAmmoCount(EntityUid uid)
    {
        var ammo = new GetAmmoCountEvent();
        RaiseLocalEvent(uid, ref ammo);

        return ammo.Count;
    }

    private List<(EntityUid? Entity, IShootable Shootable)> TakeAmmo(EntityUid uid, EntityUid user, EntityCoordinates coordinates)
    {
        var ev = new TakeAmmoEvent(1, new List<(EntityUid? Entity, IShootable Shootable)>(), coordinates, user);
        RaiseLocalEvent(uid, ev);

        return ev.Ammo;
    }
}
