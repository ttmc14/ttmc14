using Content.Shared._MC.Damage;
using Content.Shared.StatusEffectNew.Components;
using Robust.Shared.Timing;

namespace Content.Shared._MC.StatusEffects.Sleep;

public sealed partial class MCEffectSleepSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly MCDamageableSystem _damageable = null!;

    public override void Initialize()
    {
        base.Initialize();
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<MCEffectSleepComponent, StatusEffectComponent>();
        while (query.MoveNext(out _, out var component, out var effectComponent))
        {
            if (component.TickNext > _timing.CurTime)
                continue;

            component.TickNext = component.TickDelay + _timing.CurTime;

            if (effectComponent.AppliedTo is not { } targetUid)
                continue;

            var healModifier = component.HealingDamage;

            _damageable.AdjustDamage(targetUid, component.HealingDamageModifier * healModifier);
        }
    }
}
