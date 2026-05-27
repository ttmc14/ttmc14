using Content.Shared._MC.Armor.Core;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Damage;

public sealed class MCDamageableSystem : EntitySystem
{
    public static readonly ProtoId<DamageTypePrototype> DamageBruteId = "MCBrute";
    public static readonly ProtoId<DamageTypePrototype> DamageBurnId = "MCBurn";
    public static readonly ProtoId<DamageTypePrototype> DamageToxinId = "MCToxin";
    public static readonly ProtoId<DamageTypePrototype> DamageOxygenId = "MCOxygen";
    public static readonly ProtoId<DamageTypePrototype> DamageCloneId = "MCClone";

    public DamageTypePrototype DamageBrute { get; private set; } = null!;

    private DamageTypePrototype _damageBurn = null!;
    private DamageTypePrototype _damageToxin = null!;
    private DamageTypePrototype _damageOxygen = null!;
    private DamageTypePrototype _damageClone = null!;

    [Dependency] private readonly IPrototypeManager _prototype = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly MCArmorSystem _armor = null!;

    private EntityQuery<DamageableComponent> _damageableQuery;

    public override void Initialize()
    {
        base.Initialize();

        _damageableQuery = GetEntityQuery<DamageableComponent>();

        DamageBrute = _prototype.Index(DamageBruteId);
        _damageBurn = _prototype.Index(DamageBurnId);
        _damageToxin = _prototype.Index(DamageToxinId);
        _damageOxygen = _prototype.Index(DamageOxygenId);
        _damageClone = _prototype.Index(DamageCloneId);
    }

    public DamageSpecifier? DealBombDamage(EntityUid uid, int penetration, DamageSpecifier damageSpecifier, EntityUid? origin = null, EntityUid? tool = null)
    {
        if (_armor.TryGetArmor(uid, out var soft, out _))
            damageSpecifier *= MCArmorSystem.ArmorToValue(soft.Bomb, penetration: penetration);

        return _damageable.TryChangeDamage(uid, damageSpecifier, ignoreResistances: true, origin: origin, tool: tool);
    }

    public void AdjustBruteLoss(EntityUid uid, float damage)
    {
        _damageable.TryChangeDamage(uid, new DamageSpecifier(DamageBrute, FixedPoint2.New(damage)), ignoreResistances: true);
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

    public void AdjustDamage(EntityUid uid, DamageSpecifier damage)
    {
        _damageable.TryChangeDamage(uid, damage, ignoreResistances: true);
    }

    public float GetBruteLoss(EntityUid uid)
    {
        if (!_damageableQuery.TryComp(uid, out var component))
            return 0;

        if (!component.Damage.DamageDict.TryGetValue(DamageBruteId, out var damage))
            return 0;

        return damage.Float();
    }

    public float GetBurnLoss(EntityUid uid)
    {
        if (!_damageableQuery.TryComp(uid, out var component))
            return 0;

        if (!component.Damage.DamageDict.TryGetValue(DamageBurnId, out var damage))
            return 0;

        return damage.Float();
    }

    #region  Has methods

    public bool HasBruteLoss(EntityUid uid)
    {
        if (!_damageableQuery.TryComp(uid, out var component))
            return false;

        if (!component.Damage.DamageDict.TryGetValue(DamageBruteId, out var damage))
            return false;

        return damage > 0;
    }

    public bool HasBurnLoss(EntityUid uid)
    {
        if (!_damageableQuery.TryComp(uid, out var component))
            return false;

        if (!component.Damage.DamageDict.TryGetValue(DamageBurnId, out var damage))
            return false;

        return damage > 0;
    }

    #endregion
}
