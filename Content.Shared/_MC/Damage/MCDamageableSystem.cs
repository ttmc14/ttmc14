using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Damage;

public sealed class MCDamageableSystem : EntitySystem
{
    private readonly ProtoId<DamageTypePrototype> _damageBruteId = "MCBrute";
    private readonly ProtoId<DamageTypePrototype> _damageBurnId = "MCBurn";
    private readonly ProtoId<DamageTypePrototype> _damageToxinId = "MCToxin";
    private readonly ProtoId<DamageTypePrototype> _damageOxygenId = "MCOxygen";
    private readonly ProtoId<DamageTypePrototype> _damageCloneId = "MCClone";

    private DamageTypePrototype _damageBrute = null!;
    private DamageTypePrototype _damageBurn = null!;
    private DamageTypePrototype _damageToxin = null!;
    private DamageTypePrototype _damageOxygen = null!;
    private DamageTypePrototype _damageClone = null!;

    [Dependency] private readonly IPrototypeManager _prototype = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;

    private EntityQuery<DamageableComponent> _damageableQuery;

    public override void Initialize()
    {
        base.Initialize();

        _damageableQuery = GetEntityQuery<DamageableComponent>();

        _damageBrute = _prototype.Index(_damageBruteId);
        _damageBurn = _prototype.Index(_damageBurnId);
        _damageToxin = _prototype.Index(_damageToxinId);
        _damageOxygen = _prototype.Index(_damageOxygenId);
        _damageClone = _prototype.Index(_damageCloneId);
    }

    public void AdjustBruteLoss(EntityUid uid, float damage)
    {
        _damageable.TryChangeDamage(uid, new DamageSpecifier(_damageBrute, FixedPoint2.New(damage)), ignoreResistances: true);
    }

    public void AdjustBurnLoss(EntityUid uid, float damage)
    {
        _damageable.TryChangeDamage(uid, new DamageSpecifier(_damageBurn, FixedPoint2.New(damage)), ignoreResistances: true);
    }

    public void AdjustToxLoss(EntityUid uid, float damage)
    {
        _damageable.TryChangeDamage(uid, new DamageSpecifier(_damageToxin, FixedPoint2.New(damage)), ignoreResistances: true);
    }

    public void AdjustOxyLoss(EntityUid uid, float damage)
    {
        _damageable.TryChangeDamage(uid, new DamageSpecifier(_damageOxygen, FixedPoint2.New(damage)), ignoreResistances: true);
    }

    public void AdjustCloneLoss(EntityUid uid, float damage)
    {
        _damageable.TryChangeDamage(uid, new DamageSpecifier(_damageClone, FixedPoint2.New(damage)), ignoreResistances: true);
    }

    public float GetBruteLoss(EntityUid uid)
    {
        if (!_damageableQuery.TryComp(uid, out var component))
            return 0;

        if (!component.Damage.DamageDict.TryGetValue(_damageBruteId, out var damage))
            return 0;

        return damage.Float();
    }

    #region  Has methods

    public bool HasBruteLoss(EntityUid uid)
    {
        if (!_damageableQuery.TryComp(uid, out var component))
            return false;

        if (!component.Damage.DamageDict.TryGetValue(_damageBruteId, out var damage))
            return false;

        return damage > 0;
    }

    public bool HasBurnLoss(EntityUid uid)
    {
        if (!_damageableQuery.TryComp(uid, out var component))
            return false;

        if (!component.Damage.DamageDict.TryGetValue(_damageBurnId, out var damage))
            return false;

        return damage > 0;
    }

    #endregion
}
