using Content.Shared._MC.Weapon.Vali.Components;
using Content.Shared._MC.Weapon.Vali.Events.Actions;
using Content.Shared._MC.Weapon.Vali.Ui;
using Content.Shared.Actions;

namespace Content.Shared._MC.Weapon.Vali;

public sealed partial class MCWeaponValiSystem
{
    private void InitializeActions()
    {
        SubscribeLocalEvent<MCWeaponValiComponent, GetItemActionsEvent>(OnGetItemActions);

        SubscribeLocalEvent<MCWeaponValiComponent, MCWeaponValiSelectReagentAction>(OnSelectReagentAction);
        SubscribeLocalEvent<MCWeaponValiComponent, MCWeaponValiSelectReagentMessage>(OnSelectReagentMessage);
    }

    private void OnGetItemActions(Entity<MCWeaponValiComponent> entity, ref GetItemActionsEvent args)
    {
        args.AddAction(ref entity.Comp.ActionSelectReagent, entity.Comp.ActionSelectReagentId);
        Dirty(entity);
    }

    private void OnSelectReagentAction(Entity<MCWeaponValiComponent> entity, ref MCWeaponValiSelectReagentAction args)
    {
        _userInterface.TryOpenUi(entity.Owner, MCWeaponValiSelectReagentUi.Key, args.Performer);
        args.Handled = true;
    }

    private void OnSelectReagentMessage(Entity<MCWeaponValiComponent> entity, ref MCWeaponValiSelectReagentMessage args)
    {
        _userInterface.CloseUi(entity.Owner, MCWeaponValiSelectReagentUi.Key);

        if (args.ReagentId is null)
        {
            DeselectReagent(entity);
            return;
        }

        StartSelectReagentDoAfter(entity, args.Actor, args.ReagentId.Value);
    }
}
