using Content.Shared._MC.Popup;
using Content.Shared._MC.Xeno.Abilities.Boiler.Bombard.Components;
using Content.Shared._MC.Xeno.Abilities.Boiler.Bombard.Events.DoAfter;
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
            _popup.PopupLocEntServer(entity, "mc-xeno-ability-bombard-dig-up-end-cancelled");
            return;
        }

        SetDigging(entity, true, true);
    }

    private void OnRest(Entity<MCXenoBombardComponent> entity, ref XenoRestEvent args)
    {
        if (!entity.Comp.Digging || !args.Resting)
            return;

        SetDigging(entity, false, true);
    }

    private void OnMove(Entity<MCXenoBombardComponent> entity, ref MoveEvent args)
    {
        if (!entity.Comp.Digging || args.NewPosition == args.OldPosition)
            return;

        SetDigging(entity, false, true);
    }


    private void SetDigging(Entity<MCXenoBombardComponent> entity, bool state, bool popup = false)
    {
        if (popup)
            _popup.PopupLocEntServer(entity, state ? "mc-xeno-ability-bombard-dig-up-end-success" : "mc-xeno-ability-bombard-dig-up");

        entity.Comp.Digging = state;
        DirtyField(entity, entity.Comp, nameof(MCXenoBombardComponent.Digging));
    }

    private bool TryDigging(Entity<MCXenoBombardComponent> entity, EntityUid action)
    {
        if (entity.Comp.Digging)
            return false;

        _popup.PopupLocEntServer(entity, "mc-xeno-ability-bombard-dig-up-start");

        var ev = new MCXenoBombardDiggingDoAfter(action, EntityManager);
        var doAfter =  new DoAfterArgs(EntityManager, entity, entity.Comp.DiggingDuration, ev, entity)
        {
            BreakOnMove = true,
            BreakOnRest = true,
        };

        _doAfter.TryStartDoAfter(doAfter, out entity.Comp.DiggingDoAfterId);
        return true;
    }

    private bool IsBusyDigging(Entity<MCXenoBombardComponent> entity)
    {
        return _doAfter.IsRunning(entity.Comp.DiggingDoAfterId);
    }
}
