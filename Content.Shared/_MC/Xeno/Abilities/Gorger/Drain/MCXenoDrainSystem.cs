using Content.Shared._MC.Damage;
using Content.Shared._MC.Xeno.Abilities.Gorger.Drain.Components;
using Content.Shared._MC.Xeno.Abilities.Gorger.Drain.Events;
using Content.Shared._MC.Xeno.Heal;
using Content.Shared._MC.Xeno.Plasma.Systems;
using Content.Shared._MC.Xeno.Sunder;
using Content.Shared._RMC14.Emote;
using Content.Shared._RMC14.Slow;
using Content.Shared.Body.Systems;
using Content.Shared.Coordinates;
using Content.Shared.DoAfter;
using Content.Shared.Interaction.Events;
using Content.Shared.Mobs.Systems;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Gorger.Drain;

public sealed class MCXenoDrainSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedRMCEmoteSystem _emote = null!;
    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedBloodstreamSystem _blood = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = null!;

    [Dependency] private readonly MCXenoHealSystem _heal = null!;
    [Dependency] private readonly MCXenoOverhealSystem _overheal = null!;
    [Dependency] private readonly MCXenoPlasmaSystem _plasma = null!;
    [Dependency] private readonly MCDamageableSystem _damage = null!;
    [Dependency] private readonly MCXenoSunderSystem _sunder = null!;
    [Dependency] private readonly RMCSlowSystem _slow = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoDrainComponent, AttackAttemptEvent>(OnAttack);

        SubscribeLocalEvent<MCXenoDrainComponent, MCXenoDrainActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoDrainComponent, MCXenoDrainDoAfterEvent>(OnDoAfter);
    }

    private static void OnAttack(Entity<MCXenoDrainComponent> ent, ref AttackAttemptEvent args)
    {
        if (ent.Comp.Feeding)
            args.Cancel();
    }

    private void OnAction(Entity<MCXenoDrainComponent> ent, ref MCXenoDrainActionEvent args)
    {
        if (args.Handled || !IsOrganicHuman(args.Target) || !CanUseAction(ent, args.Action, args.Target, affectOnDead: true))
            return;

        var corpse = IsDead(args.Target);
        if (corpse && !NeedsHealing(ent))
            return;

        var ev = new MCXenoDrainDoAfterEvent(args.Action, EntityManager, corpse);
        var doAfter = new DoAfterArgs(EntityManager, ent, corpse ? ent.Comp.CorpseDelay : ent.Comp.Delay, ev, ent, args.Target)
        {
            DistanceThreshold = ent.Comp.Range,
            BreakOnMove = true,
            DuplicateCondition = DuplicateConditions.None,
            RequireCanInteract = false,
        };

        if (!_doAfter.TryStartDoAfter(doAfter))
            return;

        args.Handled = true;

        if (corpse)
            return;

        _emote.TryEmoteWithChat(ent, "XenoRoar");

        ent.Comp.Feeding = true;
        ent.Comp.Ticks = 0;
        Dirty(ent);

        _slow.TryRoot(args.Target, ent.Comp.Delay);
        _slow.TryRoot(ent, TimeSpan.FromSeconds(0.5));
    }

    private bool NeedsHealing(EntityUid uid) =>
        _heal.GetHealth(uid) < _heal.GetHealthAlive(uid)
        || TryComp<MCXenoOverhealComponent>(uid, out var reserve) && reserve.Amount < reserve.Maximum;

    private void OnDoAfter(Entity<MCXenoDrainComponent> ent, ref MCXenoDrainDoAfterEvent args)
    {
        var valid = !args.Cancelled && _mobState.IsAlive(ent) && args.Target is { } target
            && IsOrganicHuman(target) && IsDead(target) == args.Corpse;

        if (args.Corpse)
        {
            if (!valid || !NeedsHealing(ent))
                return;

            var amount = _heal.HealWounds(ent.Owner, ent.Comp.CorpseHealMultiplier);

            _overheal.AddOverheal(ent, amount);
            _sunder.AddSunder(ent.Owner, ent.Comp.SunderRecovery);

            args.Repeat = NeedsHealing(ent);
            return;
        }

        if (valid && args.Target is { } human)
        {
            _blood.TryModifyBloodLevel(human, -ent.Comp.BloodDrain);
            _damage.AdjustBruteLoss(human, ent.Comp.Damage);

            AnimateHit(ent, human);
            ServerSpawnAttachedTo("MCEffectDrainStab", human.ToCoordinates());

            _audio.PlayPredicted(ent.Comp.HitSound, human, ent);
            _emote.TryEmoteWithChat(human, "Scream");

            _overheal.Restore(ent, ent.Comp.HealthGain);
            _plasma.RegenPlasma(ent.Owner, ent.Comp.BloodGain);

            if (++ent.Comp.Ticks < ent.Comp.DrainTicks && !IsDead(human))
            {
                _slow.TryRoot(human, ent.Comp.Delay);

                args.Repeat = true;
                return;
            }
        }

        ent.Comp.Feeding = false;
        Dirty(ent);

        ActionSetCooldown<MCXenoDrainActionEvent>(ent, ent.Comp.Cooldown);
    }
}
