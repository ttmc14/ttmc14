using Robust.Shared.Network;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Aura;

[Virtual]
public class MCAuraSystem : EntitySystem
{
    private static readonly TimeSpan UpdateInterval = TimeSpan.FromSeconds(0.1);

    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly INetManager _net = null!;

    private EntityQuery<MCAuraComponent> _auraQuery;

    private TimeSpan _lastUpdate;

    public override void Initialize()
    {
        base.Initialize();

        _auraQuery = GetEntityQuery<MCAuraComponent>();
    }

    public override void Update(float frameTime)
    {
        var time = _timing.CurTime;
        if (_net.IsClient || _lastUpdate > time)
            return;

        _lastUpdate = time + UpdateInterval;

        var query = EntityQueryEnumerator<MCAuraComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            foreach (var auraId in component.RemoveQueue)
            {
                Remove(uid, auraId);
            }

            component.RemoveQueue.Clear();
            Dirty(uid, component);

            foreach (var (auraId, expiresAt) in component.ExpiresAt)
            {
                if (expiresAt > time)
                    continue;

                RemoveQueue(uid, auraId);
            }
        }
    }

    public bool Give(EntityUid uid, MCAuraId id, MCAuraEntry entry, TimeSpan? duration = null, bool refresh = false)
    {
        var component = EnsureComp<MCAuraComponent>(uid);

        component.Entries[id] = entry;

        if (duration.HasValue)
            component.ExpiresAt[id] = duration.Value;

        Dirty(uid, component);
        return true;
    }

    public bool Remove(EntityUid uid, MCAuraId id)
    {
        if (!_auraQuery.TryComp(uid, out var component))
            return false;

        component.ExpiresAt.Remove(id);
        Dirty(uid, component);

        return component.Entries.Remove(id);
    }

    public bool RemoveQueue(EntityUid uid, MCAuraId id)
    {
        if (!_auraQuery.TryComp(uid, out var component) || component.Entries.ContainsKey(id) || component.RemoveQueue.Contains(id))
            return false;

        component.RemoveQueue.Add(id);
        Dirty(uid, component);

        return true;
    }
}
