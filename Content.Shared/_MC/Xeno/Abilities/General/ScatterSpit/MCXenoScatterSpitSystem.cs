using Content.Shared._MC.Xeno.Spit;
using Content.Shared._RMC14.Actions;
using Content.Shared.DoAfter;

namespace Content.Shared._MC.Xeno.Abilities.General.ScatterSpit;

public sealed class MCXenoScatterSpitSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly MCSharedXenoSpitSystem _mcXenoSpit = null!;

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

        if (!RMCActions.CanUseActionPopup(entity, args.Action, entity))
            return;

        if (!_mcXenoSpit.TryGetShotCoordinates(entity, args.Target, out _, out var target))
            return;

        var ev = new MCXenoScatterSpitDoAfterEvent(GetNetCoordinates(args.Target), GetNetEntity(args.Action), GetNetEntity(args.Entity))
        {
            TargetMap = target.MapId,
        };
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

        if (!_mcXenoSpit.TryGetShotCoordinates(entity, GetCoordinates(args.Coordinates), out var origin, out var target) ||
            target.MapId != args.TargetMap)
            return;

        var action = GetEntity(args.Action);
        if (!RMCActions.TryUseAction(entity, action, entity))
            return;

        args.Handled = true;

        _mcXenoSpit.Shoot(
            entity,
            origin,
            target,
            entity.Comp.ProjectileId,
            entity.Comp.Count,
            entity.Comp.MaxDeviation,
            entity.Comp.Speed,
            entity.Comp.Sound,
            target: GetEntity(args.Entity)
        );
    }
}
