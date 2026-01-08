using Content.Shared._MC.Xeno.Pheromones.Relay;

namespace Content.Shared._MC.Xeno.Pheromones;

public sealed class MCXenoPheromonesPassiveSystem : EntitySystem
{
    [Dependency] private readonly MCXenoPheromonesRelaySystem _mcXenoPheromonesRelay = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoPheromonesPassiveComponent, ComponentStartup>(OnStartup);
    }

    private void OnStartup(Entity<MCXenoPheromonesPassiveComponent> entity, ref ComponentStartup args)
    {
        foreach (var entry in entity.Comp.Entries)
        {
            _mcXenoPheromonesRelay.AddRelayPheromones(entity, entry);
        }
    }
}
