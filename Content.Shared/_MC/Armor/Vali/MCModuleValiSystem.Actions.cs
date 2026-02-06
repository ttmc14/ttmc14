using Content.Shared._MC.Armor.Modules.Events;
using Content.Shared.Actions;

namespace Content.Shared._MC.Armor.Vali;

public sealed partial class MCModuleValiSystem
{
    private void OnGetAction(Entity<MCModuleValiComponent> entity, ref MCArmorModuleRelayedEvent<GetItemActionsEvent> args)
    {
        foreach (var actionId in entity.Comp.ActionIds)
        {
            var actionUid = entity.Comp.ActionUids.GetValueOrDefault(actionId, null);

            args.Args.AddAction(ref actionUid, actionId);
            entity.Comp.ActionUids[actionId] = actionUid;
        }

        Dirty(entity);
    }
}
