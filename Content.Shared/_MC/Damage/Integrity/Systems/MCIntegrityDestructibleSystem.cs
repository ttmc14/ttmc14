using Content.Shared._MC.Damage.Integrity.Components;
using Content.Shared._MC.Damage.Integrity.Events;
using Content.Shared.Destructible;

namespace Content.Shared._MC.Damage.Integrity.Systems;

public sealed class MCIntegrityDestructibleSystem : EntitySystem
{
    [Dependency] private readonly SharedDestructibleSystem _destructible = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCIntegrityDestructibleComponent, MCIntegrityTriggeredEvent>(OnTriggered);
    }

    private void OnTriggered(Entity<MCIntegrityDestructibleComponent> entity, ref MCIntegrityTriggeredEvent args)
    {
        if (args.IntegrityId != entity.Comp.Integrity)
            return;

        _destructible.DestroyEntity(entity);
    }
}
