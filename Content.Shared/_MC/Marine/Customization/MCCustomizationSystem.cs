using Content.Shared._MC.Marine.Customization.Components;
using Content.Shared._MC.Marine.Customization.Events;
using Content.Shared._MC.Marine.Customization.Gui;
using Content.Shared.Interaction;

namespace Content.Shared._MC.Marine.Customization;

public sealed class MCCustomizationSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCCustomizationHolderComponent, InteractUsingEvent>(OnInteractUsingEvent);

        SubscribeLocalEvent<MCCustomizationHolderComponent, MCCustomizationApplyEvent>(OnApplyCustomization);
        SubscribeLocalEvent<MCCustomizationHolderComponent, MCCustomizationApplyDoAfterEvent>(OnDoAfter);
    }

    private void OnInteractUsingEvent(Entity<MCCustomizationHolderComponent> entity, ref InteractUsingEvent args)
    {
        if (args.Handled || !entity.Comp.Paintable)
            return;

        if (!HasComp<MCCustomizationPaintComponent>(args.Used))
            return;

        var state = new MCCustomizationBuiState(entity.Comp.Variations, GetNetEntity(entity));

        _userInterface.TryOpenUi(args.Used, MCCustomizationUi.Key, args.User);
        _userInterface.SetUiState(args.Used, MCCustomizationUi.Key, state);

        args.Handled = true;
    }

    private void OnDoAfter(Entity<MCCustomizationHolderComponent> entity, ref MCCustomizationApplyDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return;

        if (!entity.Comp.Variations.TryGetValue(args.Variation, out var data))
            return;

        var ev = new MCCustomizationApplyEvent(args.Variation, data);
        RaiseLocalEvent(entity, ref ev);

        args.Handled = true;
    }

    private void OnApplyCustomization(Entity<MCCustomizationHolderComponent> entity, ref MCCustomizationApplyEvent args)
    {
        if (entity.Comp.State == args.Key)
            return;

        entity.Comp.State = args.Key;
        Dirty(entity);

        if (TryComp<AppearanceComponent>(entity, out var appearanceComponent))
            _appearance.QueueUpdate(entity, appearanceComponent);
    }
}
