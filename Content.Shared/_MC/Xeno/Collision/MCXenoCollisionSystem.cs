using Content.Shared._MC.Xeno.Hive.Systems.Main;
using Content.Shared._RMC14.Atmos;
using Robust.Shared.Physics.Events;

namespace Content.Shared._MC.Xeno.Collision;

public sealed class MCXenoCollisionSystem : EntitySystem
{
    [Dependency] private readonly MCSharedXenoHiveSystem _mcXenoHive = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoIgnoreFriendlyCollisionComponent, PreventCollideEvent>(OnIgnoreFriendlyPreventCollide, before: [ typeof(SharedRMCFlammableSystem) ]);
    }

    private void OnIgnoreFriendlyPreventCollide(Entity<MCXenoIgnoreFriendlyCollisionComponent> entity, ref PreventCollideEvent args)
    {
        if (!_mcXenoHive.FromSameHive(entity.Owner, args.OtherEntity) || args.Cancelled)
            return;

        args.Cancelled = true;
    }
}
