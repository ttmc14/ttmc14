using System.Linq;
using Content.Shared._MC.Marine.Customization.Gui;
using Content.Shared.DoAfter;
using Content.Shared.Interaction;

namespace Content.Shared._MC.Marine.Customization;

public sealed class MCCustomizationSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCCustomizationHolderComponent, InteractUsingEvent>(OnInteractUsingEvent);
        SubscribeLocalEvent<MCCustomizationHolderComponent, MCCustomizationDoAfterEvent>(OnDoAfter);

        SubscribeLocalEvent<MCCustomizationPaintComponent, MCCustomizationSelectBuiMessage>(OnSelectMessage);
    }

    private void OnInteractUsingEvent(Entity<MCCustomizationHolderComponent> entity, ref InteractUsingEvent args)
    {
        if (args.Handled)
            return;

        if (!HasComp<MCCustomizationPaintComponent>(args.Used))
            return;

        _userInterface.TryOpenUi(args.Used, MCCustomizationUi.Key, args.User);
        _userInterface.SetUiState(args.Used, MCCustomizationUi.Key, new MCCustomizationBuiState(
            entity.Comp.Variations,
            GetNetEntity(entity)
        ));

        args.Handled = true;
    }

    private void OnSelectMessage(Entity<MCCustomizationPaintComponent> entity, ref MCCustomizationSelectBuiMessage args)
    {
        var target = GetEntity(args.TargetUid);
        var doAfter = new DoAfterArgs(EntityManager, args.Actor, entity.Comp.Delay, new MCCustomizationDoAfterEvent(args.Key), target, target, entity)
        {
            BreakOnDropItem = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnDoAfter(Entity<MCCustomizationHolderComponent> entity, ref MCCustomizationDoAfterEvent args)
    {
        if (!TryComp<AppearanceComponent>(entity, out var appearanceComponent))
            return;

        entity.Comp.State = args.Variation;
        Dirty(entity);

        _appearance.QueueUpdate(entity, appearanceComponent);
    }
}
