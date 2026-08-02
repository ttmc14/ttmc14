using Content.Shared._MC.Engineering.Miners.Components;
using Content.Shared._MC.Engineering.Miners.Events.Equipment;
using Content.Shared._RMC14.Marines.Skills;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Containers;

namespace Content.Shared._MC.Engineering.Miners;

public sealed partial class MCMinerModuleSystem : EntitySystem
{
    [Dependency] private readonly SharedContainerSystem _container = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedHandsSystem _hands = null!;

    [Dependency] private readonly SkillsSystem _rmcSkills = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCMinerModuleContainerComponent, EntInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<MCMinerModuleContainerComponent, EntRemovedFromContainerMessage>(OnRemoved);

        SubscribeLocalEvent<MCMinerModuleContainerComponent, MCMinerModuleAttachedDoAfterEvent>(OnModuleAttachedDoAfter);
        SubscribeLocalEvent<MCMinerModuleContainerComponent, MCMinerModuleDeattachedDoAfterEvent>(OnModuleDeattachedDoAfter);
    }

    private void OnInserted(Entity<MCMinerModuleContainerComponent> entity, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != entity.Comp.ContainerId)
            return;

        entity.Comp.InstalledModule = args.Entity;
        Dirty(entity);
    }

    private void OnRemoved(Entity<MCMinerModuleContainerComponent> entity, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != entity.Comp.ContainerId)
            return;

        entity.Comp.InstalledModule = null;
        Dirty(entity);
    }

    private void OnModuleAttachedDoAfter(Entity<MCMinerModuleContainerComponent> entity, ref MCMinerModuleAttachedDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled || args.Used is not { } used)
            return;

        if (!CanInsert((entity.Owner, entity.Comp), used))
            return;

        TryInsertModule((entity.Owner, entity.Comp), used);
    }

    private void OnModuleDeattachedDoAfter(Entity<MCMinerModuleContainerComponent> entity, ref MCMinerModuleDeattachedDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        if (!TryRemoveModule((entity.Owner, entity.Comp), out var moduleUid))
            return;

        _hands.TryPickupAnyHand(args.User, moduleUid.Value);
    }

    private Container EnsureContainer(Entity<MCMinerModuleContainerComponent?> entity)
    {
        return !Resolve(entity, ref entity.Comp)
            ? null!
            : _container.EnsureContainer<Container>(entity, entity.Comp.ContainerId);
    }
}
