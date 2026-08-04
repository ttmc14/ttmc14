using Content.Shared._MC.Marine.Customization.Components;
using Content.Shared._MC.Marine.Customization.Events;
using Content.Shared._MC.Marine.Customization.Gui;
using Content.Shared.DoAfter;

namespace Content.Shared._MC.Marine.Customization;

public sealed class MCCustomizationPaintSystem : EntitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCCustomizationPaintComponent, MCCustomizationSelectBuiMessage>(OnSelectMessage);
    }

    private void OnSelectMessage(Entity<MCCustomizationPaintComponent> entity, ref MCCustomizationSelectBuiMessage args)
    {
        var target = GetEntity(args.TargetUid);
        var doAfter = new DoAfterArgs(EntityManager, args.Actor, entity.Comp.Delay, new MCCustomizationApplyDoAfterEvent(args.Key), target, target, entity)
        {
            BreakOnDropItem = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }
}
