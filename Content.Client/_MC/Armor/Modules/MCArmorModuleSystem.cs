using Content.Client.Clothing;
using Content.Shared._MC.Armor.Modules.Core;
using Content.Shared._MC.Armor.Modules.Core.Components;
using Content.Shared.Clothing;

namespace Content.Client._MC.Armor.Modules;

public sealed class MCArmorModuleSystem : MCArmorModuleSharedSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCArmorModularClothingComponent, GetEquipmentVisualsEvent>(OnClothingEquipmentVisuals, after: [typeof(ClientClothingSystem)]);
    }

    private void OnClothingEquipmentVisuals(Entity<MCArmorModularClothingComponent> entity, ref GetEquipmentVisualsEvent args)
    {
        foreach (var slot in entity.Comp.Slots)
        {
            if (slot.Module is not { } moduleUid || !ArmorModuleQuery.TryComp(moduleUid, out var moduleComponent))
                continue;

            if (moduleComponent.Visuals is not {} sprite)
                continue;

            var target = moduleComponent.VisualsLayer ?? args.Slot;
            var key = $"mc_armor_modular_clothing_slot_{slot.Id}";

            args.Layers.Add((key, new PrototypeLayerData
            {
                RsiPath = sprite.RsiPath.CanonPath,
                State = sprite.RsiState,
            }));

            // Depth tweak
            args.MCDepth[key] = target;
        }
    }
}
