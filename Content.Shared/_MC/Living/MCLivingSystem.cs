using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;

namespace Content.Shared._MC.Living;

public sealed class MCLivingSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = null!;

    private EntityQuery<MobStateComponent> _mobStateQuery;

    public override void Initialize()
    {
        base.Initialize();

        _mobStateQuery = GetEntityQuery<MobStateComponent>();
    }

    public int Get<T>(Predicate<Entity<T>>? predicate = null) where T : IComponent
    {
        var total = 0;
        var query = EntityQueryEnumerator<T>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (_mobStateQuery.TryComp(uid, out var mobState) && _mobState.IsDead(uid, mobState))
                continue;

            if (predicate != null && !predicate((uid, comp)))
                continue;

            total++;
        }

        return total;
    }
}
