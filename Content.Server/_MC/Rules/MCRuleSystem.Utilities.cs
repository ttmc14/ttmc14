using Robust.Shared.Map;

namespace Content.Server._MC.Rules;

public abstract partial class MCRuleSystem<T> where T : IComponent
{
    protected List<EntityUid> GetEntities<TComp>() where TComp : IComponent
    {
        var list = new List<EntityUid>();
        var query = AllEntityQuery<TComp>();
        while (query.MoveNext(out var uid, out _))
        {
            list.Add(uid);
        }

        return list;
    }

    protected List<MapId> GetEntityMapIds<TComp>() where TComp : IComponent
    {
        var list = new List<MapId>();
        var query = EntityQueryEnumerator<TComp, TransformComponent>();
        while (query.MoveNext(out _, out var transform))
        {
            list.Add(transform.MapID);
        }

        return list;
    }
}
