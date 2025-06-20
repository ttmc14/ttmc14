using Content.Shared.Weapons.Ranged.Events;
using Content.Shared.Weapons.Ranged.Systems;
using Robust.Shared.Containers;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Weapon;

public sealed class MCAmmoAccuracySystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = default!;
    [Dependency] private readonly SharedGunSystem _gunSystem = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCAmmoAccuracyComponent, EntInsertedIntoContainerMessage>(OnAmmoContainerChanged);
        SubscribeLocalEvent<MCAmmoAccuracyComponent, EntRemovedFromContainerMessage>(OnAmmoContainerChanged);
        SubscribeLocalEvent<MCAmmoAccuracyComponent, GunRefreshModifiersEvent>(OnRefreshModifiers);
    }

    private void OnAmmoContainerChanged<T>(Entity<MCAmmoAccuracyComponent> ent, ref T args) where T : ContainerModifiedMessage
    {
        if (args.Container.ID != ent.Comp.ContainerId)
            return;

        _gunSystem.RefreshModifiers(ent.Owner);
    }

    private void OnRefreshModifiers(Entity<MCAmmoAccuracyComponent> ent, ref GunRefreshModifiersEvent args)
    {
        var component = _container.GetContainer(ent, ent.Comp.ContainerId);
        var containedEntities = component.ContainedEntities;
        if (containedEntities.Count == 0)
            return;

        var bulletEntity = containedEntities[0];
        var bulletMetaData = MetaData(bulletEntity);

        foreach (var modifier in ent.Comp.Modifiers)
        {
            if (modifier.Id.Id != bulletMetaData.EntityPrototype?.ID)
                continue;

            args.MinAngle = Angle.FromDegrees(Math.Max(args.MinAngle.Degrees + modifier.ScatterFlat, 0));
            args.MaxAngle = Angle.FromDegrees(Math.Max(args.MaxAngle.Degrees + modifier.ScatterFlat, args.MinAngle));
            break;
        }
    }
}
