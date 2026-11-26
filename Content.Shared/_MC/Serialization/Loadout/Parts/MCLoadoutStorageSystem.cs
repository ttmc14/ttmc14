using System.Linq;
using Content.Shared._MC.Serialization.Loadout.Data;
using Content.Shared.Storage;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Containers;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Serialization.Loadout.Parts;

[Serializable, NetSerializable]
public sealed partial class MCLoadoutStorage : MCLoadoutPart
{
    [DataField]
    public List<MCLoadoutStorageEntry> Items = new();

    public override IEnumerable<MCLoadoutItem> GetContents() => Items.Select(entry => entry.Item);
}

[DataDefinition, Serializable, NetSerializable]
public sealed partial class MCLoadoutStorageEntry
{
    [DataField(required: true)]
    public MCLoadoutItem Item = null!;

    [DataField]
    public ItemStorageLocation? Location;
}

public sealed class MCLoadoutStorageSystem : MCLoadoutPartSystem<StorageComponent, MCLoadoutStorage>
{
    [Dependency] private readonly SharedStorageSystem _storage = null!;
    [Dependency] private readonly SharedContainerSystem _containers = null!;

    protected override MCLoadoutStorage Capture(Entity<StorageComponent> entity)
    {
        var part = new MCLoadoutStorage();
        foreach (var (uid, location) in entity.Comp.StoredItems)
        {
            if (Serializer.BuildItem(uid) is { } child)
                part.Items.Add(new MCLoadoutStorageEntry { Item = child, Location = location });
        }

        return part;
    }

    protected override void Restore(Entity<StorageComponent> entity, MCLoadoutStorage part, MCLoadoutRestoreContext context)
    {
        var defaults = new List<EntityUid>();
        foreach (var child in entity.Comp.Container.ContainedEntities.ToArray())
        {
            if (_containers.Remove(child, entity.Comp.Container))
                defaults.Add(child);
        }

        foreach (var entry in part.Items)
        {
            if (RestoreStoredItem(entry.Item, defaults, context) is not { } child)
                continue;

            if (entry.Location is { } location &&
                _storage.InsertAt(entity.Owner, child, location, out _, playSound: false, stackAutomatically: false))
                continue;

            _storage.Insert(entity, child, out _, playSound: false, stackAutomatically: false);
        }

        foreach (var unused in defaults)
            Del(unused);
    }

    private EntityUid? RestoreStoredItem(MCLoadoutItem item, List<EntityUid> defaults, MCLoadoutRestoreContext context)
    {
        var index = defaults.FindIndex(uid => MetaData(uid).EntityPrototype?.ID == item.ProtoId.Id);
        if (index < 0)
            return context.Vend(item);

        var existing = defaults[index];
        defaults.RemoveAt(index);
        context.Restore(existing, item);
        return existing;
    }
}
