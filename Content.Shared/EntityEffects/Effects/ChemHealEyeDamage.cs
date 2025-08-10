using Content.Shared.Eye.Blinding.Systems;
using Robust.Shared.Prototypes;
using Robust.Shared.GameObjects;
using Robust.Shared.Utility;

namespace Content.Shared.EntityEffects.Effects;

public sealed partial class ChemHealEyeDamage : EntityEffect
{
    [DataField]
    public float Amount = -1f;

    private static readonly Dictionary<EntityUid, float> Accumulated = new();

    protected override string? ReagentEffectGuidebookText(IPrototypeManager prototype, IEntitySystemManager entSys)
        => Loc.GetString("reagent-effect-guidebook-cure-eye-damage",
                         ("chance", Probability),
                         ("deltasign", MathF.Sign(Amount)));

    public override void Effect(EntityEffectBaseArgs args)
    {
        if (args is EntityEffectReagentArgs reagentArgs)
        {
            if (reagentArgs.Scale != 1f)
                return;
        }

        var entity = args.TargetEntity;
        var entMan = args.EntityManager;
        var blindSys = entMan.EntitySysManager.GetEntitySystem<BlindableSystem>();

        if (!Accumulated.TryGetValue(entity, out var acc))
            acc = 0f;

        acc += Amount;

        var whole = (int)acc;
        if (whole != 0)
        {
            blindSys.AdjustEyeDamage(entity, whole);
            acc -= whole;
        }

        if (MathF.Abs(acc) < 0.001f)
            Accumulated.Remove(entity);
        else
            Accumulated[entity] = acc;
    }
}
