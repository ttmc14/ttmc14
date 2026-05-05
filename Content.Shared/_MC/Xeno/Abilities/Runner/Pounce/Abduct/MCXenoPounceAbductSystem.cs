namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce.Abduct;

public sealed class MCXenoPounceAbductSystem : EntitySystem
{
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoPounceAbductComponent, MCXenoPounceHitEvent>(OnHit);
    }

    private void OnHit(Entity<MCXenoPounceAbductComponent> entity, ref MCXenoPounceHitEvent args)
    {
        if (!TryComp<MCXenoPouncingComponent>(entity, out var pouncingComponent))
            return;

        _transform.SetMapCoordinates(entity, pouncingComponent.Origin);
        _transform.SetMapCoordinates(args.TargetUid, pouncingComponent.Origin);
    }
}
