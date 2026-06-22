using System.Text.Json.Serialization;
using Content.Shared.Damage;
using Content.Shared.Damage.Prototypes;
using Content.Shared.EntityEffects;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Chemistry.Solutions.Effects;

public sealed partial class MCHealthAll : EntityEffect
{
    [DataField, JsonPropertyName("damageId")]
    public ProtoId<DamageTypePrototype> DamageId;

    private IPrototypeManager? _prototype;
    private DamageableSystem? _damageable;

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return $"Излечивает весь {prototype.Index(DamageId).LocalizedName} урон";
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is not EntityEffectReagentArgs)
            return;

        if (!args.EntityManager.TryGetComponent<DamageableComponent>(args.TargetEntity, out var damageableComponent))
            return;

        _prototype ??= IoCManager.Resolve<IPrototypeManager>();

        var damageValue = damageableComponent.Damage.DamageDict[DamageId];
        var damage = new DamageSpecifier(_prototype.Index(DamageId), damageValue);

        _damageable ??= args.EntityManager.System<DamageableSystem>();
        _damageable.TryChangeDamage(args.TargetEntity, -damage, true, false, damageableComponent);
    }
}
