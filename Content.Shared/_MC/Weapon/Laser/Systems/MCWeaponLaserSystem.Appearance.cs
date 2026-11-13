using Content.Shared._MC.Weapon.Laser.Components;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Containers;

namespace Content.Shared._MC.Weapon.Laser.Systems;

public sealed partial class MCWeaponLaserSystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;

    private void InitializeAppearance()
    {
        SubscribeLocalEvent<MCWeaponLaserComponent, EntInsertedIntoContainerMessage>(OnInsertedIntoContainer);
        SubscribeLocalEvent<MCWeaponLaserComponent, EntRemovedFromContainerMessage>(OnRemovedFromContainer);
    }

    private void OnInsertedIntoContainer(Entity<MCWeaponLaserComponent> entity, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != entity.Comp.ContainerId)
            return;

        UpdateAppearance(entity);
    }

    private void OnRemovedFromContainer(Entity<MCWeaponLaserComponent> entity, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != entity.Comp.ContainerId)
            return;

        UpdateAppearance(entity);
    }

    private void UpdateAppearance(Entity<MCWeaponLaserComponent> entity)
    {
        var hasAmmo = TryGetAmmo(entity, out _);
        _appearance.SetData(entity, AmmoVisuals.MagLoaded, hasAmmo);
    }
}
