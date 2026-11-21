using System.Linq;
using Content.Shared._MC.Armor.Modules.Core;
using Content.Shared._MC.Armor.Modules.Core.Components;
using Content.Shared._MC.Serialization.Loadout.Data;
using Robust.Shared.Serialization;

namespace Content.Shared._MC.Serialization.Loadout.Parts;

[Serializable, NetSerializable]
public sealed partial class MCLoadoutArmorModules : MCLoadoutSlotsPart
{
    // A storage module adds the storage component that receives the armor's saved contents.
    public override int RestoreOrder => -1;
}

public sealed class MCLoadoutArmorSystem : MCLoadoutSlotsSystem<MCArmorModularClothingComponent, MCLoadoutArmorModules>
{
    [Dependency] private readonly MCArmorModuleSharedSystem _armor = null!;

    protected override MCLoadoutArmorModules CreatePart() => new();

    protected override IEnumerable<Slot> GetSlots(Entity<MCArmorModularClothingComponent> entity)
    {
        foreach (var slot in entity.Comp.Slots)
            yield return new Slot(slot.Id, slot.Module, slot.Module != null);
    }

    protected override bool TryInstall(Entity<MCArmorModularClothingComponent> entity, Slot slot, EntityUid item, EntityUid user)
    {
        var armorSlot = entity.Comp.Slots.First(s => s.Id == slot.Id);
        return TryComp<MCArmorModuleComponent>(item, out var module) &&
            _armor.AttachModuleToAnySlot(entity, (item, module), null, armorSlot);
    }
}
