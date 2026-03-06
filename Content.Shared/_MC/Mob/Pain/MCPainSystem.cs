using Content.Shared.Damage;

namespace Content.Shared._MC.Mob.Pain;

public sealed partial class MCPainSystem : EntitySystem
{

    private EntityQuery<DamageableComponent> _damageableQuery;
    private EntityQuery<MCPainComponent> _painQuery;

    public override void Initialize()
    {
        base.Initialize();

        _damageableQuery = GetEntityQuery<DamageableComponent>();
        _painQuery = GetEntityQuery<MCPainComponent>();
    }

    private float GetDamageLoss(Entity<MCPainComponent> entity, DamageableComponent? damageableComponent = null)
    {
        if (!_damageableQuery.Resolve(entity, ref damageableComponent, logMissing: false))
            return 0;

        var painloss = 1f;
        foreach (var (id, value) in damageableComponent.Damage.DamageDict)
        {
            if (!entity.Comp.DamageLosseModifiers.TryGetValue(id, out var modifier))
                continue;

            painloss += modifier * value.Float();
        }

        return painloss;
    }
}
