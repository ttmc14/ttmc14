using Content.Shared._MC.Weapons.AttackModeSelection.Core.Components;
using Content.Shared._MC.Weapons.AttackModeSelection.Core.Events;
using Content.Shared._MC.Weapons.AttackModeSelection.Core.UI;
using Content.Shared.Actions;

namespace Content.Shared._MC.Weapons.AttackModeSelection.Core;

public sealed class MCAttackModeSelectionSystem : EntitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCAttackModeSelectionComponent, GetItemActionsEvent>(OnGetItemActions);
        SubscribeLocalEvent<MCAttackModeSelectionComponent, MCAttackModeSelectionAction>(OnSelectReagentAction);
        SubscribeLocalEvent<MCAttackModeSelectionComponent, MCAttackModeSelectionMessage>(OnSelectReagentMessage);
    }

    private void OnGetItemActions(Entity<MCAttackModeSelectionComponent> entity, ref GetItemActionsEvent args)
    {
        if (entity.Comp.Modes.Count <= 0)
            return;

        args.AddAction(ref entity.Comp.Action, entity.Comp.ActionId);
        Dirty(entity);
    }

    private void OnSelectReagentAction(Entity<MCAttackModeSelectionComponent> entity, ref MCAttackModeSelectionAction args)
    {
        _userInterface.TryOpenUi(entity.Owner, MCAttackModeSelectionUI.Key, args.Performer);
        args.Handled = true;
    }

    private void OnSelectReagentMessage(Entity<MCAttackModeSelectionComponent> entity, ref MCAttackModeSelectionMessage args)
    {
        _userInterface.CloseUi(entity.Owner, MCAttackModeSelectionUI.Key);

        var ev = new MCAttackModeSelectionEvent(args.Mode);
        RaiseLocalEvent(entity, ref ev);
    }
}
