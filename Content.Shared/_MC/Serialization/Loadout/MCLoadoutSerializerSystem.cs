using System.IO;
using System.Linq;
using Content.Shared._MC.Serialization.Loadout.Data;
using Content.Shared.Inventory;
using JetBrains.Annotations;
using Robust.Shared;
using Robust.Shared.Configuration;
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

    private readonly Dictionary<Type, MCLoadoutPartSystem> _handlers = new();

    internal void Register<TComponent, TPart>(MCLoadoutPartSystem<TComponent, TPart> system)
        where TComponent : Component
        where TPart : MCLoadoutPart
    {
        _handlers.Add(typeof(TPart), system);
    }

    internal void RestoreItem(EntityUid uid, MCLoadoutItem item, MCLoadoutRestoreContext context)
    {
        if (item.Parts.Count == 0)
            return;

        // Resolve each component after earlier parts have installed their modules.
        var restored = new HashSet<Type>();
        foreach (var part in item.Parts.OrderBy(p => p.RestoreOrder))
        {
            var type = part.GetType();
            if (restored.Add(type) && _handlers.TryGetValue(type, out var handler))
                handler.RestorePart(uid, part, context);
        }
    }

    [PublicAPI]
    public MCLoadout LoadoutFromStream(Stream stream)
    {
        using var reader = new StreamReader(stream, EncodingHelpers.UTF8);
        var yamlStream = new YamlStream();
        yamlStream.Load(reader);

        var root = yamlStream.Documents[0].RootNode;
        return _serialization.Read<MCLoadout>(root.ToDataNode(), notNullableOverride: true);
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
    public MCLoadoutSlot BuildSlot(EntityUid itemUid, SlotDefinition slot) =>
        new()
        {
            SlotName = slot.Name,
            Item = BuildItem(itemUid),
        };

    [PublicAPI]
    public MCLoadoutItem? BuildItem(EntityUid itemUid)
    {
        if (TerminatingOrDeleted(itemUid))
            return null;

        if (MetaData(itemUid).EntityPrototype is not { } entityPrototype)
            return null;

        var item = new MCLoadoutItem { ProtoId = entityPrototype.ID };
        foreach (var handler in _handlers.Values)
        {
            if (handler.CapturePart(itemUid) is { } part)
                item.Parts.Add(part);
        }

        return item;
    }
}
