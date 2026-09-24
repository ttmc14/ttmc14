using System.Diagnostics.CodeAnalysis;
using Content.Shared._MC.Engineering.Miners.Components;
using Content.Shared._MC.Engineering.Miners.Events;
using JetBrains.Annotations;

namespace Content.Shared._MC.Engineering.Miners;

public sealed partial class MCMinerModuleSystem
{
    [PublicAPI]
    public void RelayEvent<T>(Entity<MCMinerModuleContainerComponent> entity, ref T args)
        where T : struct
    {
        var ev = new MCMinerModuleRelayedEvent<T>(args);
        if (entity.Comp.InstalledModule is not { } moduleUid)
            return;

        RaiseLocalEvent(moduleUid, ref ev);

        args = ev.Args;
    }

    [PublicAPI]
    public bool HasModule(Entity<MCMinerModuleContainerComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp))
            return false;

        return entity.Comp.InstalledModule is not null;
    }

    [PublicAPI]
    public bool CanInsert(
        Entity<MCMinerModuleContainerComponent?> entity,
        EntityUid module)
    {
        if (!Resolve(entity, ref entity.Comp))
            return false;

        if (!_query.HasComp(module))
            return false;

        var container = EnsureContainer(entity);
        return container.ContainedEntities.Count == 0 && _container.CanInsert(module, container);
    }

    [PublicAPI]
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

    [PublicAPI]
    public bool TryInsertModule(
        Entity<MCMinerModuleContainerComponent?> entity,
        EntityUid module)
    {
        if (!CanInsert(entity, module))
            return false;

        var container = EnsureContainer(entity);
        if (!_container.Insert(module, container))
            return false;

        Dirty(entity);

        return true;
    }

    [PublicAPI]
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

        Dirty(entity);

        return true;
    }
}
