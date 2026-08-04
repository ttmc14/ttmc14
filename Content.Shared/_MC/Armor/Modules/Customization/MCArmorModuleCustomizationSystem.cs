using Content.Shared._MC.Armor.Modules.Core.Components;
using Content.Shared._MC.Marine.Customization.Events;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Armor.Modules.Customization;

public sealed class MCArmorModuleCustomizationSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCArmorModuleComponent, MCCustomizationApplyEvent>(OnApplyCustomization);
    }

    private void OnApplyCustomization(Entity<MCArmorModuleComponent> entity, ref MCCustomizationApplyEvent args)
    {
        entity.Comp.Visuals = new SpriteSpecifier.Rsi(args.Data.Path, args.Data.State);
        Dirty(entity);

        var transform = Transform(entity);
        if (!transform.ParentUid.IsValid())
            return;

        var parent = transform.ParentUid;

        if (!TryComp<AppearanceComponent>(parent, out var parentAppearance))
            return;

        _appearance.QueueUpdate(parent, parentAppearance);
    }
}
