using System.Linq;
using Content.Shared._MC.Serialization.Loadout.Data;
using Content.Shared._RMC14.Webbing;
using Content.Shared.Storage.EntitySystems;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Serialization.Loadout.Parts;

[Serializable, NetSerializable]
public sealed partial class MCLoadoutWebbing : MCLoadoutPart
{
    [DataField]
    public MCLoadoutItem? Item;

    public override IEnumerable<MCLoadoutItem> GetContents()
    {
        if (Item != null)
            yield return Item;
    }

    // Attaching webbing creates the clothing's storage.
    public override int RestoreOrder => -1;
}

public sealed class MCLoadoutWebbingSystem : MCLoadoutPartSystem<WebbingClothingComponent, MCLoadoutWebbing>
{
    [Dependency] private readonly SharedWebbingSystem _webbing = null!;
    [Dependency] private readonly SharedStorageSystem _storage = null!;

    protected override MCLoadoutWebbing? Capture(Entity<WebbingClothingComponent> clothing)
    {
        if (_webbing.HasWebbing((clothing, clothing.Comp), out var attached))
            return new MCLoadoutWebbing { Item = Serializer.BuildItem(attached) };

        // Record an empty slot when spawning the uniform would otherwise fill it.
        return clothing.Comp.StartingWebbing != null ? new MCLoadoutWebbing() : null;
    }

    protected override void Restore(Entity<WebbingClothingComponent> clothing, MCLoadoutWebbing part, MCLoadoutRestoreContext context)
    {
        EntityUid? current = _webbing.HasWebbing((clothing, clothing.Comp), out var attached) ? attached.Owner : null;
        if (part.Item is not { } saved)
        {
            if (current != null && _webbing.TryDetach(clothing, deferTransfer: false))
                Del(current);

            return;
        }

        if (GetOrAttachWebbing(clothing, current, saved, context) is not { } webbing)
            return;

        // Once the in-game transfer has run, the snapshot stores the contents on the clothing.
        // Move the prototype fill there too so storage restoration can reuse those items.
        if (saved.Parts.OfType<MCLoadoutStorage>().FirstOrDefault() is { Items.Count: 0 })
            _storage.TransferEntities(webbing, clothing);

        context.Restore(webbing, saved);
    }

    private EntityUid? GetOrAttachWebbing(Entity<WebbingClothingComponent> clothing, EntityUid? current,
        MCLoadoutItem saved, MCLoadoutRestoreContext context)
    {
        if (current is { } existing && MetaData(existing).EntityPrototype?.ID == saved.ProtoId.Id)
            return existing;

        if (context.Purchase(saved) is not { } purchased || !HasComp<WebbingComponent>(purchased))
            return null;

        if (current != null && !_webbing.TryDetach(clothing, deferTransfer: false))
            return null;

        if (_webbing.Attach(clothing, purchased, null, out _))
        {
            Del(current);
            return purchased;
        }

        if (current is { } previous)
            _webbing.Attach(clothing, previous, null, out _);

        return null;
    }
}
