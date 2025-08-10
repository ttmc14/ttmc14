namespace Content.Shared._MC;

public abstract class MCEntitySystemSingleton<TComponent> : EntitySystem where TComponent : IComponent, new()
{
    [ViewVariables]
    protected Entity<TComponent> Inst => GetInst();

    protected Entity<TComponent> GetInst()
    {
        var query = EntityQueryEnumerator<TComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            return (uid, component);
        }

        var instance = Spawn();
        return (instance, AddComp<TComponent>(instance));
    }
}
