using Content.Shared._MC.Aura;
using Content.Shared._MC.StatusEffects.Stun;
using Content.Shared._MC.Xeno.Abilities.Gorger.Carnage.Components;
using Content.Shared._MC.Xeno.Abilities.Gorger.Carnage.Events;
using Content.Shared._MC.Xeno.Abilities.Gorger.Feast.Components;
using Content.Shared._MC.Xeno.Heal;
using Content.Shared._MC.Xeno.Plasma.Systems;
using Content.Shared._RMC14.Slow;
using Content.Shared.Body.Systems;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Systems;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Audio.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Carnage;

public sealed class MCXenoCarnageSystem : MCXenoAbilitySystem<MCXenoCarnageComponent, MCXenoCarnageActionEvent>
{
    private const string AuraLayer = "carnage";

    [Dependency] private readonly MCAuraSystem _aura = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedBloodstreamSystem _blood = null!;
    [Dependency] private readonly MovementSpeedModifierSystem _movement = null!;
    [Dependency] private readonly IGameTiming _timing = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;

    [Dependency] private readonly MCXenoHealSystem _heal = null!;
    [Dependency] private readonly MCXenoOverhealSystem _overheal = null!;
    [Dependency] private readonly MCXenoPlasmaSystem _plasma = null!;
    [Dependency] private readonly RMCSlowSystem _slow = null!;
    [Dependency] private readonly MCStunSystem _stun = null!;

    public override void Initialize()
    {
        base.Initialize();
        SubscribeLocalEvent<MCXenoCarnageActiveComponent, ComponentShutdown>(OnAuraShutdown);

        SubscribeLocalEvent<MCXenoCarnageActiveComponent, RefreshMovementSpeedModifiersEvent>(OnSpeed);
        SubscribeLocalEvent<MCXenoCarnageActiveComponent, AttackAttemptEvent>(OnAttack);

        SubscribeLocalEvent<MCXenoCarnageActiveComponent, MCXenoCarnageDoAfterEvent>(OnDoAfter);
        SubscribeLocalEvent<MCXenoCarnageActiveComponent, MeleeHitEvent>(OnHit);
        SubscribeLocalEvent<MCXenoCarnageActiveComponent, MobStateChangedEvent>(OnState);
    }

    protected override void OnUse(Entity<MCXenoCarnageComponent> ent, ref MCXenoCarnageActionEvent args)
    {
        _audio.PlayPredicted(ent.Comp.RoarSound, ent, ent);

        var active = EnsureComp<MCXenoCarnageActiveComponent>(ent);

        active.Power = Math.Clamp(1 - _plasma.GetPlasmaNormalized(ent), 0.2f, 0.8f) + 0.2f;
        active.SpeedMultiplier = 1 + ent.Comp.SpeedBonus * active.Power;
        active.EndsAt = _timing.CurTime + ent.Comp.Duration;
        Dirty(ent.Owner, active);

        _aura.Set(ent, AuraLayer, ent.Comp.Aura, bloodTint: active.Power >= ent.Comp.HighThirst ? active.Power : 0);
        _movement.RefreshMovementSpeedModifiers(ent);

        ActionSetToggled<MCXenoCarnageActionEvent>(ent, true);
    }

    private static void OnSpeed(Entity<MCXenoCarnageActiveComponent> ent, ref RefreshMovementSpeedModifiersEvent args) =>
        args.ModifySpeed(ent.Comp.SpeedMultiplier);

    private static void OnAttack(Entity<MCXenoCarnageActiveComponent> ent, ref AttackAttemptEvent args)
    {
        if (ent.Comp.Feeding)
            args.Cancel();
    }

    private void OnHit(Entity<MCXenoCarnageActiveComponent> ent, ref MeleeHitEvent args)
    {
        if (!args.IsHit || args.User != ent.Owner || !TryComp<MCXenoCarnageComponent>(ent, out var ability) || ent.Comp.Feeding)
            return;

        foreach (var target in args.HitEntities)
        {
            if (!IsOrganicHuman(target) || IsDead(target))
                continue;

            var amount = _heal.GetHealthAlive(ent) * ability.HealFraction * ent.Comp.Power;

            _overheal.Restore(ent, amount, 0.5f);

            if (HasComp<MCXenoFeastActiveComponent>(ent))
                _overheal.RestoreNearby(ent, ability.HealRange, amount, 0, "MCEffectTelekinesis");

            if (ent.Comp.Power >= ability.HighThirst)
            {
                var delay = ability.FeedDelay;
                _stun.Paralyze(target, delay);
                _slow.TryRoot(ent, delay);

                var doAfter = new DoAfterArgs(EntityManager, ent, ability.FeedDelay, new MCXenoCarnageDoAfterEvent(), ent, target)
                {
                    DistanceThreshold = ability.Range,
                    BreakOnMove = true,
                    DuplicateCondition = DuplicateConditions.None,
                    RequireCanInteract = false,
                };

                if (_doAfter.TryStartDoAfter(doAfter))
                {
                    ent.Comp.Feeding = true;
                    Dirty(ent);
                    return;
                }
            }

            Stop(ent);
            return;
        }
    }

    private void OnDoAfter(Entity<MCXenoCarnageActiveComponent> ent, ref MCXenoCarnageDoAfterEvent args)
    {
        if (!args.Cancelled && ent.Comp.Feeding && _mobState.IsAlive(ent) && args.Target is { } human
            && TryComp<MCXenoCarnageComponent>(ent, out var ability) && IsOrganicHuman(human) && !IsDead(human))
        {
            _blood.TryModifyBloodLevel(human, -ability.BloodDrain);
            _plasma.RegenPlasma(ent.Owner, ability.BloodGain * ent.Comp.Power);

            AnimateHit(ent, human);
            _audio.PlayPredicted(ability.FeedSound, human, ent);
        }

        Stop(ent);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (Net.IsClient)
            return;

        var query = EntityQueryEnumerator<MCXenoCarnageActiveComponent>();
        while (query.MoveNext(out var uid, out var comp))
        {
            if (!comp.Feeding && comp.EndsAt <= _timing.CurTime)
                Stop((uid, comp));
        }
    }

    private void OnAuraShutdown(Entity<MCXenoCarnageActiveComponent> ent, ref ComponentShutdown args) =>
        _aura.Remove(ent, AuraLayer);

    private void OnState(Entity<MCXenoCarnageActiveComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Alive)
            Stop(ent);
    }

    private void Stop(Entity<MCXenoCarnageActiveComponent> ent)
    {
        RemComp<MCXenoCarnageActiveComponent>(ent);

        _movement.RefreshMovementSpeedModifiers(ent);

        ActionSetToggled<MCXenoCarnageActionEvent>(ent, false);
    }
}
