using System.Linq;
using Content.Shared._MC.Weapon.Vali.Components;
using Content.Shared._MC.Weapon.Vali.Events;
using Content.Shared.Damage;
using Content.Shared.FixedPoint;
using Content.Shared.Mobs.Components;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._MC.Weapon.Vali;

public sealed partial class MCWeaponValiSystem
{
    private void InitializeMelee()
    {
        SubscribeLocalEvent<MCWeaponValiComponent, MeleeAttackEvent>(OnMeleeAttack);
        SubscribeLocalEvent<MCWeaponValiComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeAttack(Entity<MCWeaponValiComponent> entity, ref MeleeAttackEvent _)
    {
        if (entity.Comp.Reagent is not { } selectedReagent)
            return;

        if (HasUsageAmount(entity, selectedReagent))
            return;

        DeselectReagent(entity);
    }

    private void OnMeleeHit(Entity<MCWeaponValiComponent> entity, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        var ev = new MCWeaponValiMeleeHitEvent(args.HitEntities);
        RaiseLocalEvent(args.User, ref ev);

        if (entity.Comp.Reagent is not {} selectedReagent)
            return;

        if (!HasUsageAmount(entity, selectedReagent))
        {
            DeselectReagent(entity);
            return;
        }

        if (!entity.Comp.Reagents.TryGetValue(selectedReagent, out var available) || available < entity.Comp.ReagentUsage)
            return;

        if (!args.HitEntities.Any(HasComp<MobStateComponent>))
            return;

        foreach (var hitEntity in args.HitEntities)
        {
            ApplyReagentEffects(entity, hitEntity, args.User, selectedReagent, entity.Comp.ReagentUsage, args.BaseDamage + args.BonusDamage);
        }

        entity.Comp.Reagents[selectedReagent] -= entity.Comp.ReagentUsage;

        if (!_doAfter.IsRunning(entity.Comp.ReagentSelectDoAfterId))
        {
            DeselectReagent(entity);

            if (HasUsageAmount(entity, selectedReagent))
                StartSelectReagentDoAfter(entity, args.User, selectedReagent);
        }

        Dirty(entity);
    }

    private void ApplyReagentEffects(Entity<MCWeaponValiComponent> entity, EntityUid target, EntityUid user, string reagentId, FixedPoint2 amount, DamageSpecifier damageSpecifier)
    {
        if (!entity.Comp.ReagentData.TryGetValue(reagentId, out var data))
            return;

        foreach (var effect in data.Effects)
        {
            effect.Apply(target, user, amount, damageSpecifier, EntityManager);
        }
    }
}
