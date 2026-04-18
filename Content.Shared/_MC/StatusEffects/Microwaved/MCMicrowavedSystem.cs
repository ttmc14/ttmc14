using Content.Shared._MC.Damage;
using Content.Shared.StatusEffectNew;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Timing;

namespace Content.Shared._MC.StatusEffects.Microwaved;

public sealed class MCMicrowavedSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly MCDamageableSystem _damageable = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCMicrowavedComponent, StatusEffectTimeUpdatedEvent>(OnTimeUpdated);
    }

    // It's called when we want to continue effect
    // that's mean repeat application
    // we increase power effect with each application
    private static void OnTimeUpdated(Entity<MCMicrowavedComponent> entity, ref StatusEffectTimeUpdatedEvent args)
    {
        entity.Comp.Stacks = int.Min(entity.Comp.Stacks + 1, entity.Comp.MaxStacks);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCMicrowavedComponent, StatusEffectComponent>();
        while (query.MoveNext(out _, out var component, out var effectComponent))
        {
            if (component.TickNext > _timing.CurTime)
                continue;

            component.TickNext = component.TickDelay + _timing.CurTime;

            if (effectComponent.AppliedTo is not { } targetUid)
                continue;

            var damage = component.Damage * component.Stacks;
            _damageable.AdjustDamage(targetUid, damage);
        }
    }
}
