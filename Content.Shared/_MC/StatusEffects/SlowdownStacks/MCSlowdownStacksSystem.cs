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
        SubscribeLocalEvent<MCSlowdownStacksComponent, StatusEffectRelayedEvent<RefreshMovementSpeedModifiersEvent>>(OnRefreshMovementSpeedModifiers);
        SubscribeLocalEvent<MCSlowdownStacksComponent, StatusEffectRemovedEvent>(OnRemoved);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<MCSlowdownStacksComponent, StatusEffectComponent>();
        while (query.MoveNext(out var uid, out var component, out var statusEffectComponent))
        {
            if (statusEffectComponent.AppliedTo is not { } targetUid)
                continue;

            if (component.UpdateNext > _timing.CurTime)
                continue;

            var elapsed = _timing.CurTime - component.UpdateNext + component.UpdateDelay;
            component.Stacks = MathF.Max(0f, component.Stacks - component.Regeneration * (float) elapsed.TotalSeconds);
            component.UpdateNext = _timing.CurTime + component.UpdateDelay;
            DirtyFields(uid, component, null, nameof(MCSlowdownStacksComponent.Stacks), nameof(MCSlowdownStacksComponent.UpdateNext));

            if (component.Stacks == 0f && _statusEffects.TryRemoveStatusEffect(targetUid, EffectProtoId))
                continue;

            _movementSpeedModifier.RefreshMovementSpeedModifiers(targetUid);
        }
    }

    private void OnRemoved(Entity<MCSlowdownStacksComponent> entity, ref StatusEffectRemovedEvent args)
    {
        // The effect still receives relayed events while it is being removed.
        entity.Comp.Stacks = 0f;
        _movementSpeedModifier.RefreshMovementSpeedModifiers(args.Target);
    }

    private static void OnRefreshMovementSpeedModifiers(Entity<MCSlowdownStacksComponent> entity, ref StatusEffectRelayedEvent<RefreshMovementSpeedModifiersEvent> args) =>
        args.Args.ModifySpeed(GetSpeedMultiplier(entity));

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
