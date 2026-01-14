using Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSelector.UI;
using Content.Shared.Actions;
using Content.Shared.Chemistry.Reagent;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSelector;

// TODO: [MC] Use MCXenoAbilitySystem<TComponent, TEvent>
public sealed class MCXenoReagentSelectorSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoReagentSelectorComponent, ActionAddedEvent>(OnActionAdded);
        SubscribeLocalEvent<MCXenoReagentSelectorComponent, MCXenoReagentSelectorBuiMsg>(OnSelectMessage);
        SubscribeLocalEvent<MCXenoReagentSelectorComponent, MCXenoReagentSelectorActionEvent>(OnAction);
    }

    private void OnActionAdded(Entity<MCXenoReagentSelectorComponent> entity, ref ActionAddedEvent args)
    {
        if (Actions.GetEvent(args.Action) is not MCXenoReagentSelectorActionEvent)
            return;

        if (entity.Comp.Entries.Values.Count == 0)
            return;

        foreach (var entry in entity.Comp.Entries.Values)
        {
            entity.Comp.SelectedEntry = entry;
            DirtyField(entity, entity.Comp, nameof(MCXenoReagentSelectorComponent.SelectedEntry));

            Actions.SetIcon(args.Action, entry.Sprite);
            break;
        }
    }

    public EntProtoId? GetSmoke(Entity<MCXenoReagentSelectorComponent?> entity)
    {
        return !Resolve(entity, ref entity.Comp)
            ? null
            : entity.Comp.SelectedEntry?.SmokeEntityId;
    }

    public ProtoId<ReagentPrototype>? GetReagent(Entity<MCXenoReagentSelectorComponent?> entity)
    {
        return !Resolve(entity, ref entity.Comp)
            ? null
            : entity.Comp.SelectedEntry?.ReagentId;
    }

    private void OnSelectMessage(Entity<MCXenoReagentSelectorComponent> entity, ref MCXenoReagentSelectorBuiMsg args)
    {
        Select(entity, args.Id);
        _userInterface.CloseUi(entity.Owner, MCXenoReagentSelectorUI.Key, entity);
    }

    private void OnAction(Entity<MCXenoReagentSelectorComponent> entity, ref MCXenoReagentSelectorActionEvent args)
    {
        args.Handled = true;
        _userInterface.TryOpenUi(entity.Owner, MCXenoReagentSelectorUI.Key, entity);
    }

    private void Select(Entity<MCXenoReagentSelectorComponent> entity, string id)
    {
        if (!entity.Comp.Entries.TryGetValue(id, out var entry))
        {
            Log.Warning($"Entity {id} not found");
            return;
        }

        entity.Comp.SelectedEntry = entry;
        DirtyField(entity, entity.Comp, nameof(MCXenoReagentSelectorComponent.SelectedEntry));

        foreach (var action in RMCActions.GetActionsWithEvent<MCXenoReagentSelectorActionEvent>(entity))
        {
            Actions.SetIcon(action.Owner, entry.Sprite);
        }
    }
}
