using Content.Shared.Movement.Systems;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Timing;

namespace Content.Shared._MC.StatusEffects.SlowdownStacks;

public sealed partial class MCSlowdownStacksSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = null!;
    [Dependency] private readonly SharedStatusEffectsSystem _statusEffects = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCSlowdownStacksComponent, RefreshMovementSpeedModifiersEvent>(OnRefreshMovementSpeedModifiers);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<MCSlowdownStacksComponent, StatusEffectComponent>();
        while (query.MoveNext(out var uid, out var component, out var statusEffectComponent))
        {
            if (component.UpdateNext > _timing.CurTime)
                continue;

            Update((uid, component), 1f /  (float) component.UpdateDelay.TotalSeconds, statusEffectComponent.AppliedTo);

            component.UpdateNext = _timing.CurTime + component.UpdateDelay;
            DirtyField(uid, component, nameof(MCSlowdownStacksComponent.UpdateNext));
        }
    }

    private void Update(Entity<MCSlowdownStacksComponent> entity, float scale, EntityUid? targetUid)
    {
        entity.Comp.Stacks -= entity.Comp.Regeneration * scale;

        if (targetUid is not null)
            _movementSpeedModifier.RefreshMovementSpeedModifiers(targetUid.Value);

        if (entity.Comp.Stacks > 0)
            return;

        PredictedQueueDel(entity.Owner);
    }

    private static void OnRefreshMovementSpeedModifiers(Entity<MCSlowdownStacksComponent> entity, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(GetSpeedMultiplier(entity));
    }

    private static float GetSpeedMultiplier(Entity<MCSlowdownStacksComponent> entity)
    {
        const float factor = 0.1f;
        const float minMultiplier = 0.15f;

        var stacks = entity.Comp.Stacks;

        return stacks <= 0
            ? 1f
            : float.Max(minMultiplier, float.Exp(-factor * stacks));
    }
}
