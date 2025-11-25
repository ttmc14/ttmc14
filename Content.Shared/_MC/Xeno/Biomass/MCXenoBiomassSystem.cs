using System.Runtime.CompilerServices;

namespace Content.Shared._MC.Xeno.Biomass;

public sealed class MCXenoBiomassSystem : EntitySystem
{
    private EntityQuery<MCXenoBiomassComponent> _biomassQuery;

    public override void Initialize()
    {
        base.Initialize();

        _biomassQuery = GetEntityQuery<MCXenoBiomassComponent>();
    }

    public void Add(Entity<MCXenoBiomassComponent?> entity, int value)
    {
        if (!_biomassQuery.Resolve(entity, ref entity.Comp))
            return;

        Set(entity, Get(entity) + value);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public void Set(Entity<MCXenoBiomassComponent?> entity, int value)
    {
        if (!_biomassQuery.Resolve(entity, ref entity.Comp))
            return;

        entity.Comp.Amount = value;
        Dirty(entity);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public int Get(Entity<MCXenoBiomassComponent?> entity)
    {
        return !_biomassQuery.Resolve(entity, ref entity.Comp) ? 0 : entity.Comp.Amount;
    }
}
