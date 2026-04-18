using Content.Client.Weapons.Ranged.Systems;
using Content.Shared._MC.Weapon.Laser.Components;

namespace Content.Client._MC.Weapons.Laser;

public sealed class MCWeaponLaserCounterSystem : EntitySystem
{
    [Dependency] private readonly GunSystem _gun = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCWeaponLaserComponent, AfterAutoHandleStateEvent>(OnHandle);

        SubscribeLocalEvent<MCWeaponLaserComponent, GunSystem.AmmoCounterControlEvent>(OnControl);
        SubscribeLocalEvent<MCWeaponLaserComponent, GunSystem.UpdateAmmoCounterEvent>(OnAmmoCountUpdate);
    }

    private void OnHandle(Entity<MCWeaponLaserComponent> entity, ref AfterAutoHandleStateEvent args)
    {
        _gun.UpdateAmmoCount(entity);
    }

    private static void OnControl(Entity<MCWeaponLaserComponent> entity, ref GunSystem.AmmoCounterControlEvent args)
    {
        args.Control = new GunSystem.DefaultStatusControl();
    }

    private static void OnAmmoCountUpdate(Entity<MCWeaponLaserComponent> entity, ref GunSystem.UpdateAmmoCounterEvent args)
    {
        if (args.Control is not GunSystem.DefaultStatusControl boxes)
            return;

        boxes.Update(entity.Comp.Shots, entity.Comp.Capacity);
    }
}
