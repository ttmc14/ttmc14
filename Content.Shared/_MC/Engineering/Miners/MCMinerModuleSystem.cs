using Content.Shared._MC.Engineering.Miners.Components;
using Content.Shared._MC.Engineering.Miners.Events.Equipment;
using Content.Shared._RMC14.Marines.Skills;
using Content.Shared.DoAfter;
using Content.Shared.Hands.EntitySystems;
using Robust.Shared.Containers;

namespace Content.Shared._MC.Engineering.Miners;

public sealed partial class MCMinerModuleSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    [Dependency] private readonly SharedContainerSystem _container = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedHandsSystem _hands = null!;

    [Dependency] private readonly MCMinerSystem _miner = null!;

    [Dependency] private readonly SkillsSystem _rmcSkills = null!;

    private EntityQuery<MCMinerModuleComponent> _query;
    private EntityQuery<MCMinerComponent> _moduleQuery;

    public override void Initialize()
    {
        _query = GetEntityQuery<MCMinerModuleComponent>();
        _moduleQuery = GetEntityQuery<MCMinerComponent>();

        SubscribeLocalEvent<MCMinerModuleContainerComponent, ComponentStartup>(OnStartup);

        SubscribeLocalEvent<MCMinerModuleContainerComponent, EntInsertedIntoContainerMessage>(OnInserted);
        SubscribeLocalEvent<MCMinerModuleContainerComponent, EntRemovedFromContainerMessage>(OnRemoved);

        SubscribeLocalEvent<MCMinerModuleContainerComponent, MCMinerModuleAttachedDoAfterEvent>(OnModuleAttachedDoAfter);
        SubscribeLocalEvent<MCMinerModuleContainerComponent, MCMinerModuleDeattachedDoAfterEvent>(OnModuleDeattachedDoAfter);
    }

    private void OnStartup(Entity<MCMinerModuleContainerComponent> entity, ref ComponentStartup args)
    {
        _appearance.SetData(entity, MCMinerLayers.Module, "base");
    }

    private void OnInserted(Entity<MCMinerModuleContainerComponent> entity, ref EntInsertedIntoContainerMessage args)
    {
        if (args.Container.ID != entity.Comp.ContainerId)
            return;

        entity.Comp.InstalledModule = args.Entity;
        Dirty(entity);

        var ev = new MCMinerModuleAttachedEvent(entity, args.Entity);
        RaiseLocalEvent(args.Entity, ref ev);

        _appearance.SetData(entity, MCMinerLayers.Module, Comp<MCMinerModuleComponent>(args.Entity).Appearance);
        _miner.Refresh(entity.Owner);
    }

    private void OnRemoved(Entity<MCMinerModuleContainerComponent> entity, ref EntRemovedFromContainerMessage args)
    {
        if (args.Container.ID != entity.Comp.ContainerId)
            return;

        entity.Comp.InstalledModule = null;
        Dirty(entity);

        var ev = new MCMinerModuleDeattachedEvent(entity, args.Entity);
        RaiseLocalEvent(args.Entity, ref ev);

        _appearance.SetData(entity, MCMinerLayers.Module, "base");
        _miner.Refresh(entity.Owner);
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
