using Content.Shared._MC.Xeno.Abilities.Forger.Whirlwind.Components;
using Content.Shared._MC.Xeno.Abilities.Forger.Whirlwind.Events;
using Content.Shared.DoAfter;
using Content.Shared._MC.Xeno.Spit;

namespace Content.Shared._MC.Xeno.Abilities.Forger.Whirlwind;

public sealed class MCXenoWhirlwindSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;
    [Dependency] private readonly MCSharedXenoSpitSystem _mcXenoSpit = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoWhirlwindComponent, MCXenoWhirlwindActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoWhirlwindComponent, MCXenoWhirlwindDoAfterEvent>(OnActionDoAfter);
    }

    private void OnAction(Entity<MCXenoWhirlwindComponent> entity, ref MCXenoWhirlwindActionEvent args)
    {
        if (!RMCActions.CanUseActionPopup(entity, args.Action))
            return;

        var ev = new MCXenoWhirlwindDoAfterEvent(args.Action, args.Target, EntityManager);
        _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, entity, entity.Comp.ProjectileDelay, ev, entity)
        {
            BreakOnMove = true,
        });
    }

    private void OnActionDoAfter(Entity<MCXenoWhirlwindComponent> entity, ref MCXenoWhirlwindDoAfterEvent args)
    {
        if (args.Handled || args.Cancelled)
            return;

        args.Handled = true;

        var actionUid = GetEntity(args.ActionUid);
        var coordinates = GetCoordinates(args.Coordinates);

        if (!RMCActions.TryUseAction(entity, actionUid, entity))
            return;

        _mcXenoSpit.Shoot(
            entity,
            coordinates,
            entity.Comp.ProjectileId,
            1,
            Angle.Zero,
            entity.Comp.ProjectileSpeed,
            entity.Comp.Sound
        );

        ActionStartUseDelay<MCXenoWhirlwindActionEvent>(entity, actionUid);
    }
}
