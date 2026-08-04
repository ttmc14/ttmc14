using Content.Shared._MC.Marine.Customization.Components;
using Content.Shared.Clothing.Components;
using Content.Shared.Item;
using Robust.Client.GameObjects;
using Robust.Client.ResourceManagement;
using Robust.Shared.Serialization.TypeSerializers.Implementations;

namespace Content.Client._MC.Marine.Customization;

public sealed class MCCustomizationHolderVisualizerSystem : VisualizerSystem<MCCustomizationHolderComponent>
{
    [Dependency] private readonly IResourceCache _resource = null!;

    protected override void OnAppearanceChange(EntityUid uid, MCCustomizationHolderComponent component, ref AppearanceChangeEvent args)
    {
        base.OnAppearanceChange(uid, component, ref args);

        if (component.State is not { } state)
            return;

        if (!component.Variations.TryGetValue(state, out var data))
            return;

#pragma warning disable RA0002
        if (args.Sprite?.BaseRSI is not null && _resource.TryGetResource(SpriteSpecifierSerializer.TextureRoot / data.Path, out RSIResource? baseRsi))
            SpriteSystem.SetBaseRsi(uid, baseRsi.RSI);

        if (TryComp<ClothingComponent>(uid, out var clothingComponent))
            clothingComponent.RsiPath = data.Path.ToString();

        if (TryComp<ItemComponent>(uid, out var itemComponent))
            itemComponent.RsiPath = data.Path.ToString();
#pragma warning restore RA0002
    }
}
