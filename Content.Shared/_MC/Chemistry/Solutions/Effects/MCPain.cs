using System.Text.Json.Serialization;
using Content.Shared._MC.Mob.Pain.Reagents;
using Content.Shared.EntityEffects;
using Content.Shared.FixedPoint;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Chemistry.Solutions.Effects;

public sealed partial class MCPain : EntityEffect
{
    [DataField, JsonPropertyName("pain")]
    public FixedPoint2 Pain;

    private MCPainReagentsSystem? _pain;

    protected override string ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
    {
        return Pain > 0
            ? $"Добовляет {Pain} боли"
            : $"Убирает {-Pain} боли";
    }

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is not EntityEffectReagentArgs)
            return;

        _pain ??= args.EntityManager.System<MCPainReagentsSystem>();
        _pain.EnsurePain(args.TargetEntity, Pain.Float());
    }
}
