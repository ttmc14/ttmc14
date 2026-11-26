using Content.Shared._MC.Aura;
using Content.Shared._MC.Xeno.Abilities.Gorger.PsychicLink;
using Content.Shared._MC.Xeno.Abilities.Gorger.Rejuvenate.Components;
using Content.Shared._MC.Xeno.Abilities.Gorger.Rejuvenate.Events;
using Content.Shared._MC.Xeno.Heal;
using Content.Shared._MC.Xeno.Plasma.Systems;
using Content.Shared._RMC14.Damage;
using Content.Shared.Coordinates;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs;
using Content.Shared.Movement.Systems;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Rejuvenate;

public sealed class MCXenoRejuvenateSystem : MCXenoAbilitySystem
{
    private const string AuraLayer = "rejuvenate";

    [Dependency] private readonly MCAuraSystem _aura = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = null!;

    [Dependency] private readonly MCXenoHealSystem _heal = null!;
    [Dependency] private readonly MCXenoPlasmaSystem _plasma = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoRejuvenateActiveComponent, ComponentShutdown>(OnAuraShutdown);

        SubscribeLocalEvent<MCXenoRejuvenateActiveComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);

        SubscribeLocalEvent<MCXenoRejuvenateComponent, MCXenoRejuvenateActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoRejuvenateActiveComponent, MobStateChangedEvent>(OnState);
        SubscribeLocalEvent<MCXenoRejuvenateActiveComponent, DamageModifyAfterResistEvent>(OnDamage,
            before: [typeof(MCXenoPsychicLinkSystem), typeof(MCXenoOverhealSystem)]);
    }

    private void OnAction(Entity<MCXenoRejuvenateComponent> ent, ref MCXenoRejuvenateActionEvent args)
    {
        if (args.Handled || !CanUseAction(ent, args.Action))
            return;

        if (HasComp<MCXenoRejuvenateActiveComponent>(ent))
        {
            Stop(ent);
            args.Handled = true;
            return;
        }

        if (!_plasma.HasPlasma(ent, ent.Comp.Cost))
            return;

        args.Handled = true;

        _plasma.RemovePlasma(ent.Owner, ent.Comp.Cost);

        _audio.PlayPredicted(ent.Comp.StartSound, ent, ent);
        _aura.Set(ent, AuraLayer, ent.Comp.Aura);

        var effect = EnsureComp<MCXenoRejuvenateActiveComponent>(ent);
        effect.NextTick = _timing.CurTime + ent.Comp.Interval;
        effect.SpeedMultiplier = ent.Comp.SpeedMultiplier;
        Dirty(ent.Owner, effect);

        UpdateAction(ent, true);
    }

    private void Stop(Entity<MCXenoRejuvenateComponent> ent)
    {
        RemComp<MCXenoRejuvenateActiveComponent>(ent);
        UpdateAction(ent, false);
    }

    private void UpdateAction(Entity<MCXenoRejuvenateComponent> ent, bool active)
    {
        _movement.RefreshMovementSpeedModifiers(ent);

        var delay = active ? ent.Comp.CancelDelay : ent.Comp.Cooldown;

        ActionSetToggled<MCXenoRejuvenateActionEvent>(ent, active);
        ActionSetUseDelay<MCXenoRejuvenateActionEvent>(ent, delay);
        ActionSetCooldown<MCXenoRejuvenateActionEvent>(ent, delay);
    }

    private static void OnSpeed(Entity<MCXenoRejuvenateActiveComponent> ent, ref RefreshMovementSpeedModifiersEvent args) =>
        args.ModifySpeed(ent.Comp.SpeedMultiplier);

    private void OnAuraShutdown(Entity<MCXenoRejuvenateActiveComponent> ent, ref ComponentShutdown args) =>
        _aura.Remove(ent, AuraLayer);

    private void OnState(Entity<MCXenoRejuvenateActiveComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Dead && TryComp<MCXenoRejuvenateComponent>(ent, out var ability))
            Stop((ent, ability));
    }

    private void OnDamage(Entity<MCXenoRejuvenateActiveComponent> ent, ref DamageModifyAfterResistEvent args)
    {
        if (!TryComp<MCXenoRejuvenateComponent>(ent, out var ability))
            return;

        var remaining = new DamageSpecifier(args.Damage);
        foreach (var type in MCXenoOverhealSystem.PhysicalDamageTypes)
        {
            if (!remaining.DamageDict.TryGetValue(type, out var amount) || amount <= 0)
                continue;

            ent.Comp.DamageThisTick += amount.Float();
            if (ent.Comp.DamageThisTick >= _heal.GetHealthAlive(ent) * ability.DamageThreshold)
                remaining.DamageDict[type] -= FixedPoint2.Min(amount * ability.DamageReduction, ability.MaxReduction);
        }

        args.Damage = remaining;
        Dirty(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (Net.IsClient)
            return;

        var query = EntityQueryEnumerator<MCXenoRejuvenateActiveComponent, MCXenoRejuvenateComponent>();
        while (query.MoveNext(out var uid, out var active, out var comp))
        {
            if (active.NextTick > _timing.CurTime)
                continue;

            if (!_plasma.HasPlasma(uid, comp.Cost))
            {
                Stop((uid, comp));
                continue;
            }

            active.NextTick = _timing.CurTime + comp.Interval;
            active.DamageThisTick = 0;
            Dirty(uid, active);

            _plasma.RemovePlasma(uid, comp.Cost);

            SpawnAttachedTo("MCEffectTelekinesis", uid.ToCoordinates());

            _heal.Heal(uid, _heal.GetHealthAlive(uid) * comp.HealFraction);
        }
    }
}
