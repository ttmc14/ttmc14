using Content.Shared._MC.Xeno.Spit;
using Content.Shared._RMC14.Actions;
using Content.Shared.Actions;

namespace Content.Shared._MC.Xeno.Abilities.SpitToggle;

public sealed class MCXenoSpitToggleSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly MCSharedXenoSpitSystem _xenoSpit = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoSpitToggleComponent, MCXenoSpitToggleActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoSpitToggleComponent> entity, ref MCXenoSpitToggleActionEvent args)
    {
        if (!TryComp<MCXenoSpitComponent>(entity, out var xenoSpitComponent))
            return;

        args.Handled = true;

        // Disable all other "Xeno Spit" toggle actions for this entity
        // This affects only visuals and action state, not actual logic
        foreach (var action in _rmcActions.GetActionsWithEvent<MCXenoSpitToggleActionEvent>(entity))
        {
            _actions.SetToggled((action, action), false);
        }

        if (!xenoSpitComponent.Enabled ||
            entity.Comp.ActionId is not null &&
            entity.Comp.ActionId != args.Action)
        {
            _xenoSpit.SetPreset(
                entity.Owner,
                args.ProjectileId,
                args.PlasmaCost,
                args.Delay,
                args.Speed,
                args.Sound);

            // Remember the currently active action
            entity.Comp.ActionId = args.Action;
            Dirty(entity);

            _actions.SetToggled((args.Action, args.Action), true);
            return;
        }

        // If the spit was already enabled with the same action — toggle it off
        // Clear the current active action
        entity.Comp.ActionId = null;
        Dirty(entity);

        _xenoSpit.ResetPreset(entity.Owner);
        _actions.SetToggled((args.Action, args.Action), false);
    }
}
