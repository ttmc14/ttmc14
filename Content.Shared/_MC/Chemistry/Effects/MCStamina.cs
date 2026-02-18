using System.Text.Json.Serialization;
using Content.Shared._MC.Mob.Stamina;
using Content.Shared._MC.Mob.Stamina.Components;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;
using Robust.Shared.GameObjects;

namespace Content.Shared._MC.Chemistry.Effects;

public sealed partial class MCStamina : EntityEffect
{
    [DataField, JsonPropertyName("potence")]
    public float Potence;

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        var p = Potence;
        var verb = p > 0 ? "Увеличивает" : (p < 0 ? "Уменьшает" : "Не изменяет");
        return $"{verb} выносливость на {FixedPoint2.Abs(p)}";
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is not EntityEffectReagentArgs reagentArgs)
            return;

        var entityManager = reagentArgs.EntityManager;

        var target = reagentArgs.TargetEntity;
        if (reagentArgs.OrganEntity.HasValue && entityManager.HasComponent<MCStaminaComponent>(reagentArgs.OrganEntity.Value))
            target = reagentArgs.OrganEntity.Value;

        if (!entityManager.TryGetComponent<MCStaminaComponent>(target, out var staminaComponent))
            return;

        var system = entityManager.System<MCStaminaSystem>();
        system.ApplyDamage((target, staminaComponent), Potence, updateTimer: false);
    }
}
