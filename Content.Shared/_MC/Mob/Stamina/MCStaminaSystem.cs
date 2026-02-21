using Content.Shared._MC.Mob.Stamina.Components;
using Content.Shared._MC.Stun;
using Content.Shared.Alert;
using Content.Shared.Damage;
using Content.Shared.Rejuvenate;
using Content.Shared.Rounding;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Mob.Stamina;

public sealed partial class MCStaminaSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly AlertsSystem _alerts = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly MCStunSystem _mcStun = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCStaminaComponent, ComponentStartup>(OnStaminaStartup);
        SubscribeLocalEvent<MCStaminaComponent, RejuvenateEvent>(OnStaminaRejuvenate);
    }

    private void OnStaminaStartup(Entity<MCStaminaComponent> entity, ref ComponentStartup args)
    {
        UpdateStaminaAlert(entity);
    }

    private void OnStaminaRejuvenate(Entity<MCStaminaComponent> entity, ref RejuvenateEvent args)
    {
        ApplyDamage((entity, entity.Comp), -9999, updateTimer: false);
    }

    public override void Update(float frameTime)
    {
        var query = EntityQueryEnumerator<MCStaminaComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (comp.NextRegenTime > _timing.CurTime)
                continue;

            if (comp.Stamina >= comp.StaminaMax)
                continue;

            comp.NextRegenTime = _timing.CurTime + TimeSpan.FromSeconds(1.0);

            ApplyDamage((uid, comp), -comp.StaminaMax * 0.2f * comp.RegenMultiplier);
        }
    }

    private void CheckExhaustion(Entity<MCStaminaComponent> entity)
    {
        if (entity.Comp.Stamina > entity.Comp.ExhaustionThreshold)
            return;

        if (_timing.CurTime < entity.Comp.LastExhaustionTime)
            return;

        entity.Comp.LastExhaustionTime = _timing.CurTime + entity.Comp.ExhaustionCooldown;

        _mcStun.Paralyze(entity, TimeSpan.FromSeconds(1));
        _mcStun.Slowdown(entity, TimeSpan.FromSeconds(6));
        _mcStun.Stagger(entity, TimeSpan.FromSeconds(10));

        // _status.TryAddStatusEffect(entity, "Blurry", TimeSpan.FromSeconds(3), true, "Blurry");
    }

    private void UpdateStaminaAlert(Entity<MCStaminaComponent> entity)
    {
        var level = ContentHelpers.RoundToEqualLevels(entity.Comp.Stamina, entity.Comp.StaminaMax, 5);
        _alerts.ShowAlert(entity, entity.Comp.StaminaAlert, (short) level);
    }
}
