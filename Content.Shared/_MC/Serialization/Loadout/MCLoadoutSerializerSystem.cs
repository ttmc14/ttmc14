using System.IO;
using System.Linq;
using Content.Shared._MC.Serialization.Loadout.Data;
using Content.Shared.Inventory;
using Content.Shared.Storage;
using Content.Shared.Storage.Components;
using JetBrains.Annotations;
using Robust.Shared;
using Robust.Shared.Configuration;
using Robust.Shared.Prototypes;
using Robust.Shared.Serialization.Manager;
using Robust.Shared.Serialization.Markdown;
using Robust.Shared.Utility;
using YamlDotNet.RepresentationModel;

namespace Content.Shared._MC.Serialization.Loadout;

public sealed class MCLoadoutSerializerSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configuration = null!;
    [Dependency] private readonly ISerializationManager _serialization = null!;

    [Dependency] private readonly InventorySystem _inventory = null!;

    [PublicAPI]
    public MCLoadout LoadoutFromStream(Stream stream)
    {
        using var reader = new StreamReader(stream, EncodingHelpers.UTF8);
        var yamlStream = new YamlStream();
        yamlStream.Load(reader);

        var root = yamlStream.Documents[0].RootNode;
        var export = _serialization.Read<MCLoadout>(root.ToDataNode(), notNullableOverride: true);
        return export;
    }

    [PublicAPI]
    public DataNode LoadoutToDataNode(MCLoadout loadout)
    {
        return _serialization.WriteValue(loadout, alwaysWrite: true, notNullableOverride: true);
    }

    [PublicAPI]
    public MCLoadout BuildEntity(EntityUid targetUid)
    {
        var result = new MCLoadout
        {
            ForkId = _configuration.GetCVar(CVars.BuildForkId),
        };

        var query = _inventory.GetSlotEnumerator(targetUid);
        while (query.NextItem(out var itemUid, out var definition))
        {
            result.Slots.Add(BuildSlot(itemUid, definition));
        }

        return result;
    }

    [PublicAPI]
    public MCLoadoutSlot BuildSlot(EntityUid itemUid, SlotDefinition slot)
    {
        return new MCLoadoutSlot
        {
            SlotName = slot.Name,
            Item = BuildItem(itemUid),
        };
    }

    [PublicAPI]
    public MCLoadoutItem? BuildItem(EntityUid itemUid)
    {
        if (TerminatingOrDeleted(itemUid))
            return null;

        if (MetaData(itemUid).EntityPrototype is not { } entityPrototype)
            return null;

        if (!TryComp<StorageComponent>(itemUid, out var storage) || storage.StoredItems.Count == 0)
        {
            return new MCLoadoutItem
            {
                ProtoId = entityPrototype.ID,
            };
        }

        var contents = new List<EntProtoId>();
        if (TryComp<StorageFillComponent>(itemUid, out var fillComponent))
        {
            contents = fillComponent.Contents
                .Where(e => e.PrototypeId is not null)
                .SelectMany(e => Enumerable.Repeat((EntProtoId) e.PrototypeId!, e.Amount))
                .ToList();
        }

        var contains = new List<MCLoadoutItem>();
        foreach (var (storedUid, _) in storage.StoredItems)
        {
            var prototypeId = MetaData(storedUid).EntityPrototype?.ID;
            if (string.IsNullOrEmpty(prototypeId))
                continue;

            if (contents.Contains(prototypeId))
            {
                contents.Remove(prototypeId);
                continue;
            }

            var serialized = BuildItem(storedUid);
            if (serialized is null)
                continue;

            contains.Add(serialized);
        }

        return new MCLoadoutItem
        {
            ProtoId = entityPrototype.ID,
            Contains = contains,
        };
    }
}
