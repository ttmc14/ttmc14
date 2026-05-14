using Content.Shared._RMC14.Xenonids;
using Robust.Shared.Physics.Events;

namespace Content.Shared._MC.Weapons.Projectiles.Ignore;

public sealed class MCProjectileIgnoreXenoSystem : EntitySystem
{
    private EntityQuery<XenoComponent> _xenoQuery;

    public override void Initialize()
    {
        _xenoQuery = GetEntityQuery<XenoComponent>();

        SubscribeLocalEvent<MCProjectileIgnoreXenoComponent, PreventCollideEvent>(OnPreventCollide);
    }

    private void OnPreventCollide(Entity<MCProjectileIgnoreXenoComponent> entity, ref PreventCollideEvent args)
    {
        if (!_xenoQuery.HasComp(args.OtherEntity))
            return;

        args.Cancelled = true;
    }
}
