using System.Linq;
using Content.Shared._MC.Serialization.Loadout.Data;

namespace Content.Shared._MC.Serialization.Loadout;

public abstract class MCLoadoutSlotsSystem<TComponent, TPart> : MCLoadoutPartSystem<TComponent, TPart>
    where TComponent : Component
    where TPart : MCLoadoutSlotsPart
{
    protected readonly record struct Slot(string Id, EntityUid? Item, bool Locked);

    // A generic new TPart() compiles to Activator.CreateInstance, which the client sandbox rejects.
    protected abstract TPart CreatePart();
    protected abstract IEnumerable<Slot> GetSlots(Entity<TComponent> entity);
    protected abstract bool TryInstall(Entity<TComponent> entity, Slot slot, EntityUid item, EntityUid user);
    protected virtual bool TryRemove(Entity<TComponent> entity, Slot slot, EntityUid item, EntityUid user) => false;

    protected sealed override TPart Capture(Entity<TComponent> entity)
    {
        var part = CreatePart();
        foreach (var slot in GetSlots(entity))
        {
            if (slot.Item is { } uid && Serializer.BuildItem(uid) is { } item)
                part.Slots.Add(slot.Id, item);
        }

        return part;
    }

    protected sealed override void Restore(Entity<TComponent> entity, TPart part, MCLoadoutRestoreContext context)
    {
        foreach (var slot in GetSlots(entity).ToArray())
        {
            if (part.Slots.GetValueOrDefault(slot.Id) is not { } saved)
            {
                if (!slot.Locked && slot.Item is { } old && TryRemove(entity, slot, old, context.User))
                    Del(old);

                continue;
            }

            if (slot.Item is { } current && MetaData(current).EntityPrototype?.ID == saved.ProtoId.Id)
            {
                context.Restore(current, saved);
                continue;
            }

            if (slot.Locked)
                continue;

            if (context.Purchase(saved) is not { } child || !TryInstall(entity, slot, child, context.User))
                continue;

            Del(slot.Item);
            context.Restore(child, saved);
        }
    }
}
