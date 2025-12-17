using Content.Shared._RMC14.Actions;

namespace Content.Shared._MC.Xeno.Abilities.Mark;

public sealed class MCXenoMarkSystem : EntitySystem
{
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoMarkComponent, MCXenoMarkActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoMarkComponent> entity, ref MCXenoMarkActionEvent args)
    {
        if (args.Handled)
            return;

        if (_rmcActions.TryUseAction(entity, args.Action, args.Target))
            return;
    }
}
