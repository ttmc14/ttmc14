using System.Diagnostics.CodeAnalysis;
using Content.Shared._MC.Engineering.Miners.Components;
using Content.Shared._MC.Engineering.Miners.Events;
using Content.Shared._MC.Engineering.Miners.Events.Equipment;

namespace Content.Shared._MC.Engineering.Miners;

public sealed partial class MCMinerModuleSystem
{
    public void RelayEvent<T>(Entity<MCMinerModuleContainerComponent> entity, ref T args)
    {
        var ev = new MCMinerModuleRelayedEvent<T>(args);
        if (entity.Comp.InstalledModule is not { } moduleUid)
            return;

        RaiseLocalEvent(moduleUid, ev);
    }

    public bool HasModule(Entity<MCMinerModuleContainerComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp))
            return false;

        return entity.Comp.InstalledModule is not null;
    }

    public bool CanInsert(
        Entity<MCMinerModuleContainerComponent?> entity,
        EntityUid module)
    {
        if (!Resolve(entity, ref entity.Comp))
            return false;

        if (!HasComp<MCMinerModuleComponent>(module))
            return false;

        var container = EnsureContainer(entity);
        return container.ContainedEntities.Count == 0 && _container.CanInsert(module, container);
    }

    public bool TryGetModule(
        Entity<MCMinerModuleContainerComponent?> entity,
        [NotNullWhen(true)] out EntityUid? module)
    {
        module = null;

        if (!Resolve(entity, ref entity.Comp))
            return false;

        var container = EnsureContainer(entity);
        if (container.ContainedEntities.Count == 0)
            return false;

        module = container.ContainedEntities[0];
        return true;
    }

    public bool TryInsertModule(
        Entity<MCMinerModuleContainerComponent?> entity,
        EntityUid module)
    {
        if (!CanInsert(entity, module))
            return false;

        var container = EnsureContainer(entity);

        if (!_container.Insert(module, container))
            return false;

        var ev = new MCMinerModuleAttachedEvent((entity, entity.Comp!), module);
        RaiseLocalEvent(module, ref ev);

        Dirty(entity);
        return true;
    }

    public bool TryRemoveModule(
        Entity<MCMinerModuleContainerComponent?> entity,
        [NotNullWhen(true)] out EntityUid? module)
    {
        module = null;

        if (!Resolve(entity, ref entity.Comp))
            return false;

        if (!TryGetModule(entity, out var uid))
            return false;

        if (!_container.TryRemoveFromContainer(uid.Value))
            return false;

        module = uid;

        var ev = new MCMinerModuleDeattachedEvent((entity, entity.Comp), uid.Value);
        RaiseLocalEvent(uid.Value, ref ev);

        Dirty(entity);
        return true;
    }
}
