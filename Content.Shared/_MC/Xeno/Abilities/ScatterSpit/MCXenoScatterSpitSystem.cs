using Content.Shared._MC.Xeno.Spit;
using Content.Shared._RMC14.Actions;
using Content.Shared.DoAfter;

namespace Content.Shared._MC.Xeno.Abilities.ScatterSpit;

public sealed class MCXenoScatterSpitSystem : EntitySystem
{
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly MCSharedXenoSpitSystem _mcXenoSpit = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoScatterSpitComponent, MCXenoScatterSpitActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoScatterSpitComponent, MCXenoScatterSpitDoAfterEvent>(OnDoAfter);
    }

    private void OnAction(Entity<MCXenoScatterSpitComponent> entity, ref MCXenoScatterSpitActionEvent args)
    {
        if (args.Handled)
            return;

        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
            return;

        args.Handled = true;

        var ev = new MCXenoScatterSpitDoAfterEvent(GetNetCoordinates(args.Target), GetNetEntity(args.Entity));
        var doAfter = new DoAfterArgs(EntityManager, entity, entity.Comp.Delay, ev, entity)
        {
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnDoAfter(Entity<MCXenoScatterSpitComponent> entity, ref MCXenoScatterSpitDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;

        _mcXenoSpit.Shoot(
            entity,
            GetCoordinates(args.Coordinates),
            entity.Comp.ProjectileId,
            entity.Comp.Count,
            entity.Comp.MaxDeviation,
            entity.Comp.Speed,
            entity.Comp.Sound,
            target: GetEntity(args.Entity)
        );
    }
}
