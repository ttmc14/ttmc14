using System.Linq;
using Content.Shared._MC.Mob.Pain.Core.Components;
using Content.Shared.Damage;

namespace Content.Shared._MC.Mob.Pain.Core;

public sealed partial class MCPainSystem : EntitySystem
{
    private EntityQuery<DamageableComponent> _damageableQuery;
    private EntityQuery<MCPainComponent> _query;

    public override void Initialize()
    {
        _damageableQuery = GetEntityQuery<DamageableComponent>();
        _query = GetEntityQuery<MCPainComponent>();
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<MCPainComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            component.Painloss = GetDamageLoss((uid, component)) + component.Modifiers.Sum();
            component.Modifiers.Clear();

            Dirty(uid, component);
        }
    }

    private float GetDamageLoss(Entity<MCPainComponent> entity, DamageableComponent? damageableComponent = null)
    {
        if (!_damageableQuery.Resolve(entity, ref damageableComponent, logMissing: false))
            return 0;

        var painloss = 0f;
        foreach (var (id, value) in damageableComponent.Damage.DamageDict)
        {
            if (!entity.Comp.DamageLossModifiers.TryGetValue(id, out var modifier))
                continue;

            painloss += modifier * value.Float();
        }

        return painloss;
    }
}
