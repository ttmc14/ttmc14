using Content.Shared._RMC14.Movement;
using Content.Shared._RMC14.Stun;
using Content.Shared.Administration.Logs;
using Content.Shared.Alert;
using Content.Shared.Database;
using Content.Shared.Damage;
using Content.Shared.Movement.Systems;
using Content.Shared.Movement.Components;
using Content.Shared.Projectiles;
using Content.Shared.Rejuvenate;
using Content.Shared.Speech.EntitySystems;
using Content.Shared.StatusEffect;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Wieldable.Components;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Timing;
using System.Linq;
using Robust.Shared.Configuration;

namespace Content.Shared._MC.Stamina;

public sealed class MCStaminaSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = default!;
    [Dependency] private readonly IGameTiming _timing = default!;

    [Dependency] private readonly AlertsSystem _alerts = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    [Dependency] private readonly RMCSizeStunSystem _sizeStun = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCStaminaComponent, ComponentStartup>(OnStaminaStartup);
        SubscribeLocalEvent<MCStaminaComponent, RejuvenateEvent>(OnStaminaRejuvenate);

        SubscribeLocalEvent<MCStaminaDamageOnHitComponent, MeleeHitEvent>(OnStaminaOnHit);

        SubscribeLocalEvent<MCStaminaDamageOnCollideComponent, ProjectileHitEvent>(OnStaminaOnProjectileHit);
        SubscribeLocalEvent<MCStaminaDamageOnCollideComponent, ThrowDoHitEvent>(OnStaminaOnThrowHit);
    }

    private void OnStaminaStartup(Entity<MCStaminaComponent> ent, ref ComponentStartup args)
    {
        SetStaminaAlert(ent);
    }

    private void OnStaminaRejuvenate(Entity<MCStaminaComponent> ent, ref RejuvenateEvent args)
    {
        Damage((ent, ent.Comp), -ent.Comp.Max, false);
    }

    public override void Update(float frameTime)
    {
        if (_net.IsClient)
            return;

        var time = _timing.CurTime;

        var query = EntityQueryEnumerator<MCStaminaComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (Math.Abs(component.Current - component.Max) < double.Epsilon)
                continue;

            if (TryComp<MCStaminaActiveComponent>(uid, out var active))
            {
                if (active.ZeroSprintLock && TryComp<InputMoverComponent>(uid, out var input) && input.Sprinting)
                    continue;
            }

            if (time >= component.NextRegen)
                Damage((uid, component), -component.RegenPerTick);

            if (component.Current > component.DamageThresholds)
                continue;

            var spec = new DamageSpecifier
            {
                DamageDict =
                {
                    ["MCOxygen"] = component.DamageMultiplier * frameTime,
                },
            };

            _damageable.TryChangeDamage(uid, spec, true, false);
        }
    }

    public void Damage(Entity<MCStaminaComponent?> ent, float amount, bool visual = true, bool belowZero = true, bool updateRegenTimer = true)
    {
        if (!Resolve(ent, ref ent.Comp, false))
            return;

        ent.Comp.Current = float.Clamp(ent.Comp.Current - amount, belowZero ? ent.Comp.Min : 0, ent.Comp.Max);

        if (ent.Comp.Current <= -10)
            _sizeStun.TryKnockOut(ent, TimeSpan.FromSeconds(5));

        if (updateRegenTimer)
            ent.Comp.NextRegen = _timing.CurTime + (amount > 0 ? ent.Comp.RestPeriod : ent.Comp.TimeBetweenChecks);

        SetStaminaAlert((ent, ent.Comp));
    }

    private void OnStaminaOnHit(Entity<MCStaminaDamageOnHitComponent> ent, ref MeleeHitEvent args)
    {
        if (ent.Comp.RequiresWield && TryComp<WieldableComponent>(ent.Owner, out var wieldable) && !wieldable.Wielded)
            return;

        foreach (var target in args.HitEntities)
        {
            if (!TryComp<MCStaminaComponent>(target, out var stamina))
                continue;

            Damage((target, stamina), ent.Comp.Damage);
        }
    }

    private void OnStaminaOnProjectileHit(Entity<MCStaminaDamageOnCollideComponent> ent, ref ProjectileHitEvent args)
    {
        OnCollide(ent, args.Target);
    }

    private void OnStaminaOnThrowHit(Entity<MCStaminaDamageOnCollideComponent> ent, ref ThrowDoHitEvent args)
    {
        OnCollide(ent, args.Target);
    }

    private void OnCollide(Entity<MCStaminaDamageOnCollideComponent> ent, EntityUid target)
    {
        if (!TryComp<MCStaminaComponent>(target, out var stamina))
            return;

        Damage((target, stamina), ent.Comp.Damage);
    }

    private void SetStaminaAlert(Entity<MCStaminaComponent> ent)
    {
        var level = 0;
        var thresholds = ent.Comp.TierThresholds;

        if (thresholds.Length > 0)
        {
            for (var i = 0; i < thresholds.Length; i++)
            {
                if (ent.Comp.Current <= thresholds[i])
                    level = i;
            }

            _alerts.ShowAlert(ent, ent.Comp.StaminaAlert, (short) (thresholds.Length - 1 - level));
            return;
        }

        _alerts.ShowAlert(ent, ent.Comp.StaminaAlert, 0);
    }

    public float GetDamage(EntityUid target)
    {
        if (!TryComp<MCStaminaComponent>(target, out var stamina))
            return 0;

        return float.Max(0, stamina.Max - stamina.Current);
    }
}
