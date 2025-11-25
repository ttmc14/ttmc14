using System.Linq;
using Content.Shared._MC.Damage;
using Content.Shared._MC.Flammable;
using Content.Shared._MC.Knockback;
using Content.Shared._MC.Stun;
using Content.Shared._MC.Xeno.Plasma;
using Content.Shared._MC.Xeno.Sunder;
using Content.Shared.Chemistry.Reagent;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee.Events;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Weapon.Vali;

public sealed partial class MCWeaponValiSystem
{
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly MCKnockbackSystem _mcKnockback = null!;
    [Dependency] private readonly MCDamageableSystem _mcDamageable = null!;
    [Dependency] private readonly MCStunSystem _mcStun = null!;
    [Dependency] private readonly MCXenoPlasmaSystem _mcXenoPlasma = null!;
    [Dependency] private readonly MCXenoSunderSystem _mcXenoSunder = null!;
    [Dependency] private readonly MCSharedFlammableSystem _mcFlammable = null!;

    private void OnMeleeAttack(Entity<MCWeaponValiComponent> entity, ref MeleeAttackEvent args)
    {
        if (entity.Comp.SelectedReagent is not {} selectedReagent)
            return;

        CanUse(entity, selectedReagent);
    }

    private void OnMeleeHit(Entity<MCWeaponValiComponent> entity, ref MeleeHitEvent args)
    {
        if (!args.HitEntities.Any(HasComp<MobStateComponent>))
            return;

        if (entity.Comp.SelectedReagent is not {} selectedReagent)
            return;

        if (!CanUse(entity, selectedReagent))
            return;

        entity.Comp.Reagents[selectedReagent] -= entity.Comp.ReagentUsage;
        Dirty(entity);

        switch (selectedReagent)
        {
            case "MCBicaridine":
                ProcessBicaridine(entity, args);
                break;

            case "MCKelotane":
                ProcessKelotaneEffect(entity, args);
                break;

            case "MCTricordrazine":
                ProcessTricordrazine(entity, args);
                break;

            case "MCTramadol":
                ProcessTramadol(entity, args);
                break;

            case "MCDexalin":
                ProcessDexalin(entity, args);
                break;
        }

        var additionalDamage = (args.BaseDamage + args.BonusDamage) * 0.6f;
        foreach (var targetUid in args.HitEntities)
        {
            _damageable.TryChangeDamage(targetUid, additionalDamage, ignoreResistances: true, origin: args.User, tool: entity);
        }
    }

    private bool CanUse(Entity<MCWeaponValiComponent> entity, ProtoId<ReagentPrototype> reagentId)
    {
        if (!entity.Comp.Reagents.TryGetValue(reagentId, out var quantity))
            return false;

        if (quantity < entity.Comp.ReagentUsage)
        {
            SelectReagent(entity, null);
            return false;
        }

        return true;
    }

    private void ProcessBicaridine(Entity<MCWeaponValiComponent> _, MeleeHitEvent args)
    {
        foreach (var targetUid in args.HitEntities)
        {
            _mcKnockback.KnockbackFrom(targetUid, args.User, 0.5f, 5f);
            _mcDamageable.AdjustBruteLoss(args.User, -10f);
        }
    }

    private void ProcessKelotaneEffect(Entity<MCWeaponValiComponent> _, MeleeHitEvent args)
    {
        foreach (var targetUid in args.HitEntities)
        {
            _mcFlammable.AdjustFireStacks(targetUid, 10, ignite: true);
        }
    }

    private void ProcessTricordrazine(Entity<MCWeaponValiComponent> _, MeleeHitEvent args)
    {
        foreach (var targetUid in args.HitEntities)
        {
            _mcXenoSunder.AddSunder(targetUid, -7.5f);
        }
    }

    private void ProcessTramadol(Entity<MCWeaponValiComponent> _, MeleeHitEvent args)
    {
        foreach (var targetUid in args.HitEntities)
        {
            _mcStun.Slowdown(targetUid, TimeSpan.FromSeconds(1.5f));
        }
    }

    private void ProcessDexalin(Entity<MCWeaponValiComponent> _, MeleeHitEvent args)
    {
        foreach (var targetUid in args.HitEntities)
        {
            _mcXenoPlasma.TryRemovePlasma(targetUid, 25 + _mcXenoPlasma.GetMaxPlasma(targetUid) * 0.1f);
        }
    }
}
