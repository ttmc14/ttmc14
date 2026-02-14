using System.Linq;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Dynamics;

namespace Content.Shared._MC.Physics;

public sealed class MCPhysicsFixtureCacheSystem : EntitySystem
{
    private EntityQuery<MCPhysicsFixtureCacheComponent> _cacheQuery;
    private EntityQuery<FixturesComponent> _fixturesQuery;

    public override void Initialize()
    {
        _cacheQuery = GetEntityQuery<MCPhysicsFixtureCacheComponent>();
        _fixturesQuery = GetEntityQuery<FixturesComponent>();
    }

    public Fixture? GetFirstFixture(EntityUid uid)
    {
        if (_cacheQuery.TryGetComponent(uid, out var cache) && cache.Initialized)
            return cache.CachedFixture;

        return CacheFixture(uid).CachedFixture;
    }

    private MCPhysicsFixtureCacheComponent CacheFixture(EntityUid uid)
    {
        var cache = EnsureComp<MCPhysicsFixtureCacheComponent>(uid);

        cache.Ready = true;
        cache.CachedFixture = null;

        DirtyFields(uid, cache, null, nameof(MCPhysicsFixtureCacheComponent.Ready), nameof(MCPhysicsFixtureCacheComponent.CachedFixture));

        if (!_fixturesQuery.TryGetComponent(uid, out var fixtures) || fixtures.Fixtures.Count == 0)
            return cache;

        cache.CachedFixture = fixtures.Fixtures.Values.First();
        return cache;
    }
}
