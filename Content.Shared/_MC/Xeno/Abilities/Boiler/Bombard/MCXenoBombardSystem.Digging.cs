using Content.Shared._RMC14.Xenonids.Rest;
using Content.Shared.DoAfter;

namespace Content.Shared._MC.Xeno.Abilities.Boiler.Bombard;

public sealed partial class MCXenoBombardSystem
{
    private void InitializeDigging()
    {
        SubscribeLocalEvent<MCXenoBombardComponent, MCXenoBombardDiggingDoAfter>(OnDiggingDoAfter);
        SubscribeLocalEvent<MCXenoBombardComponent, XenoRestEvent>(OnRest);
        SubscribeLocalEvent<MCXenoBombardComponent, MoveEvent>(OnMove);
    }

    private void OnDiggingDoAfter(Entity<MCXenoBombardComponent> entity, ref MCXenoBombardDiggingDoAfter args)
    {
        entity.Comp.DiggingDoAfterId = null;

        if (args.Handled)
            return;

        if (args.Cancelled)
        {
            Popup(entity, "mc-xeno-ability-bombard-dig-up-end-cancelled");
            return;
        }

        FinishDigging(entity);
    }

    private void OnRest(Entity<MCXenoBombardComponent> entity, ref XenoRestEvent args)
    {
        if (!entity.Comp.Digging || !args.Resting)
            return;

        CancelDigging(entity);
    }

    private void OnMove(Entity<MCXenoBombardComponent> entity, ref MoveEvent args)
    {
        if (!entity.Comp.Digging || args.NewPosition == args.OldPosition)
            return;

        CancelDigging(entity);
    }

    private bool IsBusyDigging(Entity<MCXenoBombardComponent> entity)
    {
        return _doAfter.IsRunning(entity.Comp.DiggingDoAfterId);
    }

    private void FinishDigging(Entity<MCXenoBombardComponent> entity)
    {
        Popup(entity, "mc-xeno-ability-bombard-dig-up-end-success");
        entity.Comp.Digging = true;
        DirtyField(entity, entity.Comp, nameof(MCXenoBombardComponent.Digging));
    }

    private void CancelDigging(Entity<MCXenoBombardComponent> entity)
    {
        Popup(entity, "mc-xeno-ability-bombard-dig-up");
        entity.Comp.Digging = false;
        DirtyField(entity, entity.Comp, nameof(MCXenoBombardComponent.Digging));
    }

    private bool TryDigging(Entity<MCXenoBombardComponent> entity, EntityUid action)
    {
        if (entity.Comp.Digging)
            return false;

        Popup(entity, "mc-xeno-ability-bombard-dig-up-start");

        var ev = new MCXenoBombardDiggingDoAfter(action, EntityManager);
        var doAfter = CreateDiggingDoAfter(entity, ev);

        _doAfter.TryStartDoAfter(doAfter, out var id);
        entity.Comp.DiggingDoAfterId = id;

        return true;
    }

    private DoAfterArgs CreateDiggingDoAfter(Entity<MCXenoBombardComponent> entity, MCXenoBombardDiggingDoAfter ev)
    {
        return new DoAfterArgs(EntityManager, entity, entity.Comp.DiggingDuration, ev, entity)
        {
            BreakOnMove = true,
            BreakOnRest = true,
        };
    }
}
