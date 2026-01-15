using Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSelector.Events;
using Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSelector.Events.Action;
using Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSelector.UI;
using Content.Shared.Actions;

namespace Content.Shared._MC.Xeno.Abilities.Defiler.ReagentSelector;

// TODO: [MC] Use MCXenoAbilitySystem<TComponent, TEvent>
public sealed partial class MCXenoReagentSelectorSystem : MCXenoAbilitySystem
{
    private static readonly Enum UIKey = MCXenoReagentSelectorUI.Key;

    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoReagentSelectorComponent, MCXenoReagentSelectorActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoReagentSelectorComponent, MCXenoReagentSelectorBuiMsg>(OnSelect);

        SubscribeLocalEvent<MCXenoReagentSelectorComponent, ActionAddedEvent>(OnActionAdded);
    }

    private void OnAction(Entity<MCXenoReagentSelectorComponent> entity, ref MCXenoReagentSelectorActionEvent args)
    {
        args.Handled = true;

        _userInterface.TryOpenUi(entity.Owner, UIKey, entity);
    }

    private void OnSelect(Entity<MCXenoReagentSelectorComponent> entity, ref MCXenoReagentSelectorBuiMsg args)
    {
        SetEntry((entity, entity), args.Id);
        RefreshActionIcon((entity, entity));

        _userInterface.CloseUi(entity.Owner, UIKey, entity);
    }

    private void OnActionAdded(Entity<MCXenoReagentSelectorComponent> entity, ref ActionAddedEvent args)
    {
        if (Actions.GetEvent(args.Action) is not MCXenoReagentSelectorActionEvent)
            return;

        if (entity.Comp.Entries.Count == 0)
        {
            Log.Warning("No entries found");
            return;
        }

        foreach (var (key, entry) in entity.Comp.Entries)
        {
            SetEntry((entity, entity), key);

            // Technically we don't have action yet, because GetActionsWithEvent can't get our event
            Actions.SetIcon(args.Action, entry.Sprite);
            break;
        }
    }

    public void SetEntry(Entity<MCXenoReagentSelectorComponent?> entity, string id)
    {
        if (!Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        if (!entity.Comp.Entries.TryGetValue(id, out var entry))
        {
            Log.Warning($"Entity {id} not found");
            return;
        }

        entity.Comp.SelectedEntry = entry;
        DirtyField(entity, entity.Comp, nameof(MCXenoReagentSelectorComponent.SelectedEntry));

        var ev = new MCXenoReagentSelectorSetEvent(id);
        RaiseLocalEvent(entity, ref ev);
    }

    private void RefreshActionIcon(Entity<MCXenoReagentSelectorComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        if (entity.Comp.SelectedEntry is not { } entry)
        {
            Log.Warning("Trying refresh icons with null entry");
            return;
        }

        foreach (var action in RMCActions.GetActionsWithEvent<MCXenoReagentSelectorActionEvent>(entity))
        {
            Actions.SetIcon(action.Owner, entry.Sprite);
        }
    }
}
