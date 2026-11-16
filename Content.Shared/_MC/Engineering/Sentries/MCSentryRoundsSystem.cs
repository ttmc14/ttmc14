using Content.Shared._MC.Engineering.Sentries.Components;
using Content.Shared.Weapons.Ranged.Components;
using Content.Shared.Weapons.Ranged.Events;
using Robust.Shared.Containers;
using Robust.Shared.Network;

namespace Content.Shared._MC.Engineering.Sentries;

public sealed class MCSentryRoundsSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly SharedContainerSystem _container = null!;

    [Dependency] private readonly MCSentryUISystem _mcSentryUi = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCSentryRoundsComponent, EntInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<MCSentryRoundsComponent, EntRemovedFromContainerMessage>(OnRemoved);
        SubscribeLocalEvent<MCSentryRoundsComponent, AmmoShotEvent>(OnAmmoShot);
    }

    private void OnInserted(Entity<MCSentryRoundsComponent> entity, ref EntInsertedIntoContainerMessage args)
    {
        if (_net.IsClient)
            return;

        if (entity.Comp.ContainerId != args.Container.ID)
            return;

        if (!TryComp<BallisticAmmoProviderComponent>(args.Entity, out var ammoProviderComponent))
            return;

        entity.Comp.Rounds = ammoProviderComponent.Count;
        entity.Comp.MaxRounds = ammoProviderComponent.Capacity;
        Dirty(entity);

        _mcSentryUi.Refresh(entity.Owner);
    }

    private void OnRemoved(Entity<MCSentryRoundsComponent> entity, ref EntRemovedFromContainerMessage args)
    {
        if (_net.IsClient)
            return;

        if (entity.Comp.ContainerId != args.Container.ID)
            return;

        if (!HasComp<BallisticAmmoProviderComponent>(args.Entity))
            return;

        entity.Comp.Rounds = 0;
        entity.Comp.MaxRounds = 0;
        Dirty(entity);

        _mcSentryUi.Refresh(entity.Owner);
    }

    private void OnAmmoShot(Entity<MCSentryRoundsComponent> entity, ref AmmoShotEvent args)
    {
        if (_net.IsClient)
            return;

        if (!_container.TryGetContainer(entity, entity.Comp.ContainerId, out var container))
            return;

        if (container.ContainedEntities.Count == 0)
            return;

        if (!TryComp<BallisticAmmoProviderComponent>(container.ContainedEntities[0], out var ammoProviderComponent))
            return;

        entity.Comp.Rounds = ammoProviderComponent.Count;
        entity.Comp.MaxRounds = ammoProviderComponent.Capacity;
        Dirty(entity);

        _mcSentryUi.Refresh(entity.Owner);
    }
}
