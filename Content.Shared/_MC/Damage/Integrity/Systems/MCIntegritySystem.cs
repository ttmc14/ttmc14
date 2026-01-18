using Content.Shared._MC.Damage.Integrity.Components;
using Content.Shared._MC.Damage.Integrity.Events;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Damage.Integrity.Systems;

public sealed class MCIntegritySystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = null!;

    private EntityQuery<DamageableComponent> _damageableQuery;

    public override void Initialize()
    {
        base.Initialize();

        _damageableQuery = GetEntityQuery<DamageableComponent>();

        SubscribeLocalEvent<MCIntegrityComponent, DamageChangedEvent>(OnDamageChanged);
    }

    public void ResetIntegrity(Entity<MCIntegrityComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp) || !_damageableQuery.TryGetComponent(entity, out var damageableComponent))
            return;

        _damageable.SetAllDamage(entity, damageableComponent, FixedPoint2.Zero);
        Dirty(entity, damageableComponent);
    }

    public void SetIntegrity(Entity<MCIntegrityComponent?> entity, ProtoId<MCIntegrityPrototype> integrityId, DamageTypePrototype damageType)
    {
        if (!Resolve(entity, ref entity.Comp) || !_damageableQuery.TryGetComponent(entity, out var damageableComponent))
            return;

        if (!entity.Comp.Thresholds.TryGetValue(integrityId, out var value))
            return;

        _damageable.SetDamage(entity, damageableComponent, new DamageSpecifier(damageType, value));
        Dirty(entity, damageableComponent);
    }

    public FixedPoint2 GetIntegrity(Entity<MCIntegrityComponent?> entity, ProtoId<MCIntegrityPrototype> integrityId)
    {
        if (!Resolve(entity, ref entity.Comp) || entity.Comp.Thresholds.TryGetValue(integrityId, out var value))
            return FixedPoint2.Zero;

        return value;
    }

    public FixedPoint2 GetTotalDamage(EntityUid entity)
    {
        return !_damageableQuery.TryGetComponent(entity, out var damageableComponent) ? FixedPoint2.Zero : damageableComponent.TotalDamage;
    }

    public string GetDamageMessage(Entity<MCIntegrityComponent?> entity, ProtoId<MCIntegrityPrototype> integrityId)
    {
        if (!Resolve(entity, ref entity.Comp) || !_damageableQuery.TryGetComponent(entity, out var damageableComponent))
            return string.Empty;

        return entity.Comp.Thresholds.TryGetValue(integrityId, out var value)
            ? $"{value - damageableComponent.TotalDamage}/{value}"
            : string.Empty;
    }

    private void OnDamageChanged(Entity<MCIntegrityComponent> entity, ref DamageChangedEvent args)
    {
        if (!_damageableQuery.TryGetComponent(entity, out var damageableComponent))
            return;

        if (!args.DamageIncreased)
            return;

        var damage = damageableComponent.TotalDamage;
        var selectedId = new ProtoId<MCIntegrityPrototype>(string.Empty);

        foreach (var (id, value) in entity.Comp.Thresholds)
        {
            if (damage >= value)
                selectedId = id;
        }

        if (selectedId == string.Empty)
            return;

        var ev = new MCIntegrityTriggeredEvent(selectedId);
        RaiseLocalEvent(entity, ref ev);
    }
}
