using Content.Shared._MC.Xeno.Spit;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Xeno.Abilities.General.SpitOnce;

public sealed class MCXenoSpitOnceSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly MCSharedXenoSpitSystem _mcXenoSpit = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoSpitOnceComponent, MCXenoSpitOnceActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoSpitOnceComponent> entity, ref MCXenoSpitOnceActionEvent args)
    {
        if (args.Handled)
            return;

        if (!TryUseAction(entity, args.Action))
            return;

        SpitOnce(
            entity,
            args.Action,
            args.ProjectileId,
            args.Target,
            args.Entity,
            args.Speed,
            args.Range
        );
    }

    private void SpitOnce(
        Entity<MCXenoSpitOnceComponent> entity,
        EntityUid actionUid,
        EntProtoId projectileId,
        EntityCoordinates targetCoordinates,
        EntityUid? targetUid,
        float projectileSpeed,
        float? projectileDistance
        )
    {
        _mcXenoSpit.Shoot(
            entity,
            targetCoordinates,
            projectileId,
            1,
            Angle.Zero,
            projectileSpeed,
            fixedDistance: projectileDistance,
            target: targetUid
        );

        ActionStartUseDelay<MCXenoSpitOnceActionEvent>(entity, actionUid);
    }
}
