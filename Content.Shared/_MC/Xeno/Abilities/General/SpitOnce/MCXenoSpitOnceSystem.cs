using Content.Shared._MC.Xeno.Spit;

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

        if (!_mcXenoSpit.TryGetShotCoordinates(entity, args.Target, out var origin, out var target))
            return;

        if (!TryUseAction(entity, args.Action))
            return;

        _mcXenoSpit.Shoot(
            entity,
            origin,
            target,
            args.ProjectileId,
            1,
            Angle.Zero,
            args.Speed,
            fixedDistance: args.Range,
            target: args.Entity
        );

        ActionStartUseDelay<MCXenoSpitOnceActionEvent>(entity, args.Action);
    }
}
