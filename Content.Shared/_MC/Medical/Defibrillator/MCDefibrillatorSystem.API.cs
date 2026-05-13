using Content.Shared._MC.Medical.Defibrillator.Components;
using Content.Shared._RMC14.Medical.Defibrillator;
using Content.Shared._RMC14.TrainingDummy;
using Content.Shared.Damage;
using Content.Shared.DoAfter;
using Content.Shared.Inventory;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using JetBrains.Annotations;
using Robust.Shared.Player;

namespace Content.Shared._MC.Medical.Defibrillator;

public sealed partial class MCDefibrillatorSystem
{
    [PublicAPI]
    public TimeSpan GetDoAfter(Entity<MCDefibrillatorComponent> entity, EntityUid user)
    {
        var skill = _rmcSkills.GetSkill(user, entity.Comp.SkillId);
        if (skill >= entity.Comp.SkillLevel)
            return entity.Comp.DoAfterBase;

        var delay = entity.Comp.DoAfterBase;
        delay +=  entity.Comp.DoAfterUnskilledPenalty;
        delay -= entity.Comp.DoAfterSkillReduction * skill;

        return delay;
    }

    [PublicAPI]
    public float GetHealAmount(Entity<MCDefibrillatorComponent> entity, EntityUid user)
    {
        var skill = _rmcSkills.GetSkill(user, entity.Comp.SkillId);
        return skill <= 0
            ? entity.Comp.HealBaseValue
            : skill * entity.Comp.SkillHealMultiplier;
    }

    [PublicAPI]
    public void TryApply(Entity<MCDefibrillatorComponent> entity, EntityUid target, EntityUid user)
    {
        if (!CanUse(entity, target, user))
            return;

        // Only for server (WIP)
        if (!_mcPowerCellLegacy.TryUseActivatableCharge(entity.Owner, user: user))
            return;

        _audio.PlayPredicted(entity.Comp.EffectSoundZap, entity, user);

        _useDelay.SetLength(entity.Owner, entity.Comp.UsageDelay, entity.Comp.UsageDelayId);
        _useDelay.TryResetDelay(entity.Owner, id: entity.Comp.UsageDelayId);

        HandleSpecialCases(entity, target, user);

        if (!TryReviveTarget(entity, target, user, out var session, out var dead))
            return;

        var sound = dead || session is null
            ? entity.Comp.EffectSoundFailure
            : entity.Comp.EffectSoundSuccess;

        _audio.PlayPvs(sound, entity);

        // If we don't have enough power left for another shot, turn it off
        if (!_mcPowerCellLegacy.HasActivatableCharge(entity.Owner))
            _toggle.TryDeactivate(entity.Owner);
    }

    [PublicAPI]
    public bool TryStart(Entity<MCDefibrillatorComponent> entity, EntityUid target, EntityUid user)
    {
        if (!CanUse(entity, target, user))
            return false;

        StartChargingAudio(entity);

        var delay = GetDoAfter(entity, user);
        var ev = new MCDefibrillatorApplyDoAfterEvent();

        return _doAfter.TryStartDoAfter(new DoAfterArgs(EntityManager, user, delay, ev, entity, target, entity)
        {
            NeedHand = true,
            BreakOnMove = true,
            BreakOnHandChange = false,
            DuplicateCondition = DuplicateConditions.SameEvent,
            TargetEffect = "RMCEffectHealBusy",
            MovementThreshold = 0.5f,
            RootEntity = true,
        });
    }

    [PublicAPI]
    public bool CanUse(Entity<MCDefibrillatorComponent> entity, EntityUid target, EntityUid? user = null)
    {
        if (!_toggle.IsActivated(entity.Owner))
        {
            if (user is not null)
                _popup.PopupClient(Loc.GetString("defibrillator-not-on"), entity, user.Value);

            return false;
        }

        if (_useDelay.IsDelayed(entity.Owner, entity.Comp.UsageDelayId))
            return false;

        // Only for server (WIP)
        if (!_mcPowerCellLegacy.HasActivatableCharge(entity.Owner, user: user))
            return false;

        if (!CanUseMobState(entity, target, user))
            return false;

        if (!CanUseBlocked(entity, target, user))
            return false;

        return true;
    }

    [PublicAPI]
    public bool CanUseMobState(Entity<MCDefibrillatorComponent> entity, EntityUid target, EntityUid? user = null)
    {
        if (!TryComp<MobStateComponent>(target, out var mobState))
            return false;

        return _mobState.IsDead(target, mobState);
    }

    [PublicAPI]
    public bool CanUseBlocked(Entity<MCDefibrillatorComponent> entity, EntityUid target, EntityUid? user = null)
    {
        if (HasComp<MCDefibrillatorNoBlockComponent>(entity))
            return true;

        var slots = _inventory.GetSlotEnumerator(target, SlotFlags.OUTERCLOTHING);
        while (slots.MoveNext(out var slot))
        {
            if (!TryComp<RMCDefibrillatorBlockedComponent>(slot.ContainedEntity, out var blockedComponent))
                continue;

            if (user is not null)
                _popup.PopupEntity(Loc.GetString(blockedComponent.Popup, ("target", target)), entity, user.Value);

            return false;
        }

        return true;
    }

    [PublicAPI]
    public bool TryReviveTarget(
        Entity<MCDefibrillatorComponent> entity,
        EntityUid targetUid,
        EntityUid user,
        out ICommonSession? session,
        out bool dead)
    {
        dead = true;
        session = null;

        // Heal
        if (_mobState.IsDead(targetUid))
        {
            var heal = entity.Comp.HealTypes * GetHealAmount(entity, user);
            _damageable.TryChangeDamage(targetUid, -heal, true, origin: entity);
        }

        // Dead -> Crit
        if (_mobThreshold.TryGetThresholdForState(targetUid, MobState.Dead, out var threshold) &&
            TryComp<DamageableComponent>(targetUid, out var damageable) && damageable.TotalDamage < threshold)
        {
            _mobState.ChangeMobState(targetUid, MobState.Critical, origin: entity);
            dead = false;
        }

        if (_mcRevive.SendReviveRequest(targetUid))
            return true;

        if (!HasComp<RMCTrainingDummyComponent>(targetUid))
            SendMessage(entity, Loc.GetString("defibrillator-not-on-mob"));

        return true;
    }
}
