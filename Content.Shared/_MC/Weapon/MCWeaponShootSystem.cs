using Content.Shared._MC.Weapon.Events;
using Content.Shared.DoAfter;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
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

        if (entity.Comp.Ready)
        {
            entity.Comp.Ready = false;
            Dirty(entity);
            return;
        }

        if (_net.IsServer)
        {
            var doAfter = new DoAfterArgs(EntityManager, args.User, entity.Comp.Delay, new MCWeaponFireDelayDoAfter(), args.Used, used: args.Used)
            {
                BreakOnMove = true,
            };

            _doAfter.TryStartDoAfter(doAfter);
        }

        args.Cancel();
    }

    private void OnAttemptShootDoAfter(Entity<MCWeaponFireDelayComponent> entity, ref MCWeaponFireDelayDoAfter args)
    {
        if (args.Handled || args.Cancelled || !TryComp<GunComponent>(args.Used, out var gunComponent))
            return;

        args.Handled = true;

        entity.Comp.Ready = true;
        Dirty(entity);

        _gun.AttemptShoot(args.User, gunComponent);
    }
}
