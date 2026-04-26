using Content.Shared._MC.Knockback;
using Content.Shared._MC.Mob.Stamina;
using Content.Shared._MC.Stun;
using Content.Shared._MC.Xeno.Abilities.Warrior.Agility;
using Content.Shared._RMC14.CameraShake;
using Content.Shared._RMC14.Weapons.Melee;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Damage;
using Content.Shared.Jittering;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Movement.Pulling.Components;
using Content.Shared.Popups;
using Robust.Shared.Audio.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Warrior.Punch;

public sealed class MCXenoPunchSystem : MCXenoAbilitySystem
{
    private static readonly LocId LocIdTargetDead = "mc-xeno-punch-target-dead";
    private static readonly LocId LocIdCannotPunch = "mc-xeno-punch-cannot-punch";
    private static readonly LocId LocIdCannotDamage = "mc-xeno-punch-cannot-damage";

    [Dependency] private readonly SharedAudioSystem _audio = null!;
    [Dependency] private readonly SharedPopupSystem _popup = null!;
    [Dependency] private readonly MobStateSystem _mobState = null!;
    [Dependency] private readonly SharedJitteringSystem _jittering = null!;
    [Dependency] private readonly RMCCameraShakeSystem _cameraShake = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;

    [Dependency] private readonly MCStunSystem _mcStun = null!;
    [Dependency] private readonly MCStaminaSystem _mcStamina = null!;
    [Dependency] private readonly MCKnockbackSystem _mcKnockback = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoPunchComponent, MCXenoPunchActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoPunchComponent> entity, ref MCXenoPunchActionEvent args)
    {
        if (!TryUse(entity, ref args))
            return;

        // TODO: empower
        const bool empowered = false;

        var empowerMultiplier = empowered ? entity.Comp.EmpowerMultiplier : 1;
        var grappled = TryComp<PullerComponent>(entity, out var pullerComponent) && pullerComponent.Pulling == args.Target;

        var damage = GetDamage(entity);
        var damageTotal = damage.GetTotal().Float();

        var sound = entity.Comp.Sound;

        var structureDamageMultiplier = HasComp<MobStateComponent>(args.Target) ? 1 : 4;
        var damageMultiplier = entity.Comp.DamageMultiplier;
        var slowdown = entity.Comp.SlowdownTime;
        var stagger = entity.Comp.StaggerTime;

        if (grappled)
        {
            sound = entity.Comp.GrappledSound;

            damageMultiplier = entity.Comp.GrappledDamageMultiplier;

            slowdown *= entity.Comp.GrappledDebuffMultiplier;
            stagger *= entity.Comp.GrappledDebuffMultiplier;

            _mcStun.Paralyze(args.Target, entity.Comp.GrappledParalyzeTime);
        }

        _jittering.DoJitter(args.Target, entity.Comp.ShakeTime, refresh: true);

        _cameraShake.ShakeCamera(args.Target, 1, 1);

        _mcStun.Slowdown(args.Target, slowdown);
        _mcStun.Stagger(args.Target, stagger);

        // TODO: blur

        _damageable.TryChangeDamage(args.Target, damage * damageMultiplier * empowerMultiplier * structureDamageMultiplier, origin: entity, tool: entity);
        _mcStamina.ApplyDamage(args.Target, damageTotal * damageMultiplier * empowerMultiplier * structureDamageMultiplier);
        _mcKnockback.KnockbackFrom(args.Target, entity, entity.Comp.KnockbackDistance, entity.Comp.KnockbackSpeed);

        // Effects
        AnimateHit(entity, args.Target);

        _audio.PlayPredicted(sound, entity, entity);
    }

    private bool TryUse(Entity<MCXenoPunchComponent> entity, ref MCXenoPunchActionEvent args)
    {
        if (args.Handled)
            return false;

        if (!HasComp<DamageableComponent>(args.Target))
        {
            PopupClient(LocIdCannotDamage);
            return false;
        }

        if (MCXenoHive.FromSameHive(entity.Owner, args.Target))
        {
            PopupClient(LocIdCannotPunch);
            return false;
        }

        if (_mobState.IsDead(args.Target))
        {
            PopupClient(LocIdTargetDead);
            return false;
        }

        if (!RMCActions.TryUseAction(entity, args.Action, entity))
            return false;

        RemComp<MCXenoAgilityActiveComponent>(entity);

        args.Handled = true;
        return true;

        void PopupClient(LocId message)
        {
            _popup.PopupClient(Loc.GetString(message), entity, entity);
        }
    }
}
