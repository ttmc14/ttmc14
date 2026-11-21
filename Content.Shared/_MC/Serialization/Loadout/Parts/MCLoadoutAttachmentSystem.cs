using Content.Shared._MC.Serialization.Loadout.Data;
using Content.Shared._RMC14.Attachable.Components;
using Content.Shared._RMC14.Attachable.Systems;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Serialization.Loadout.Parts;

[Serializable, NetSerializable]
public sealed partial class MCLoadoutAttachments : MCLoadoutSlotsPart;

public sealed class MCLoadoutAttachmentSystem : MCLoadoutSlotsSystem<AttachableHolderComponent, MCLoadoutAttachments>
{
    [Dependency] private readonly AttachableHolderSystem _attachables = null!;

    protected override MCLoadoutAttachments CreatePart() => new();

    protected override IEnumerable<Slot> GetSlots(Entity<AttachableHolderComponent> entity)
    {
        foreach (var (id, slot) in entity.Comp.Slots)
        {
            EntityUid? item = _attachables.TryGetAttachable(entity, id, out var attachment) ? attachment.Owner : null;
            yield return new Slot(id, item, slot.Locked);
        }
    }

    protected override bool TryInstall(Entity<AttachableHolderComponent> entity, Slot slot, EntityUid item, EntityUid user)
        => _attachables.Attach(entity, item, user, slot.Id);

    protected override bool TryRemove(Entity<AttachableHolderComponent> entity, Slot slot, EntityUid item, EntityUid user)
        => _attachables.Detach(entity, item, user, slot.Id);
}
