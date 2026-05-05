using Robust.Shared.Prototypes;

namespace Content.Shared._MC.StatusEffects.SlowdownStacks;

public sealed partial class MCSlowdownStacksSystem
{
    private static readonly EntProtoId EffectProtoId = "MCStatusEffectSlowdownStacks";

    public void AdjustSlowdown(EntityUid targetUid, float stacks)
    {
        if (!_statusEffects.TrySetStatusEffectDuration(targetUid, EffectProtoId, out var statusEffect))
            return;

        var component = EnsureComp<MCSlowdownStacksComponent>(statusEffect.Value);

        component.Stacks = stacks;
        component.UpdateNext = _timing.CurTime + component.UpdateDelay;

        DirtyFields(statusEffect.Value, component, null, nameof(MCSlowdownStacksComponent.Stacks), nameof(MCSlowdownStacksComponent.UpdateNext));
    }
}
