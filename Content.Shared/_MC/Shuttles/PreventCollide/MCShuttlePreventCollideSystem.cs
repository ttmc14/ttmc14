using Content.Shared._MC.Shuttles.PreventCollide.Components;
using Robust.Shared.Physics.Events;

namespace Content.Shared._MC.Shuttles.PreventCollide;

public sealed class MCShuttlePreventCollideSystem : EntitySystem
{
    private EntityQuery<MCShuttlePreventCollideComponent> _query;

    public override void Initialize()
    {
        _query = GetEntityQuery<MCShuttlePreventCollideComponent>();

        SubscribeLocalEvent<MCShuttlePreventCollideComponent, PreventCollideEvent>(OnPrevetCollide);
    }

    private void OnPrevetCollide(Entity<MCShuttlePreventCollideComponent> entity, ref PreventCollideEvent args)
    {
        if (!_query.HasComponent(args.OtherEntity))
            return;

        args.Cancelled = true;
    }
}
