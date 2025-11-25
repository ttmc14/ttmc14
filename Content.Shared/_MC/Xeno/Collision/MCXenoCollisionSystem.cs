using Content.Shared._RMC14.Atmos;
using Content.Shared._RMC14.Xenonids.Hive;
using Robust.Shared.Physics.Events;

namespace Content.Shared._MC.Xeno.Collision;

public sealed class MCXenoCollisionSystem : EntitySystem
{
    [Dependency] private readonly SharedXenoHiveSystem _rmcXenoHive = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoIgnoreFriendlyCollisionComponent, PreventCollideEvent>(OnIgnoreFriendlyPreventCollide, before: [ typeof(SharedRMCFlammableSystem) ]);
    }

    private void OnIgnoreFriendlyPreventCollide(Entity<MCXenoIgnoreFriendlyCollisionComponent> entity, ref PreventCollideEvent args)
    {
        if (!_rmcXenoHive.FromSameHive(entity.Owner, args.OtherEntity) || args.Cancelled)
            return;

        args.Cancelled = true;
    }
}
