using Content.Shared._MC.Damage.Integrity.Components;
using Content.Shared._MC.Damage.Integrity.Events;
using Content.Shared.Damage;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Damage.Integrity.Systems;

public sealed class MCIntegritySystem : EntitySystem
{
    private EntityQuery<DamageableComponent> _damageableQuery;

    public override void Initialize()
    {
        base.Initialize();

        _damageableQuery = GetEntityQuery<DamageableComponent>();

        SubscribeLocalEvent<MCIntegrityComponent, DamageChangedEvent>(OnDamageChanged);
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
