using Content.Shared._MC.Armor.Vali.Events;
using Content.Shared._MC.Popup;
using Content.Shared.Popups;

namespace Content.Shared._MC.Armor.Vali;

public sealed partial class MCModuleValiSystem
{
    private void UpdateBoostedState(Entity<MCModuleValiComponent> entity)
    {
        UpdateNecrosisLogic(entity);

        if (_timing.CurTime < entity.Comp.ResourceDrainNext)
            return;

        entity.Comp.ResourceDrainNext = _timing.CurTime + entity.Comp.ResourceDrainTime;
        ProcessBoost(entity);
    }

    private void UpdateNecrosisLogic(Entity<MCModuleValiComponent> entity)
    {
        if (entity.Comp.NecrosisStartTime is null)
            return;

        var duration = _timing.CurTime - entity.Comp.NecrosisStartTime.Value;
        var user = _mcArmorModule.GetUser(entity);

        if (duration >= entity.Comp.NecrosisWarningThreshold && entity.Comp.NecrosisStage < 1)
            SendNecrosisAlert(user, "mc-module-vali-necrosis-warning", 1, entity);

        if (duration >= entity.Comp.NecrosisDangerThreshold && entity.Comp.NecrosisStage < 2)
            SendNecrosisAlert(user, "mc-module-vali-necrosis-danger", 2, entity);

        if (duration >= entity.Comp.NecrosisThreshold && entity.Comp.NecrosisStage < 3)
            SendNecrosisAlert(user, "mc-module-vali-necrosis-final", 3, entity);
    }

    private void SendNecrosisAlert(EntityUid? user, string locId, int stage, Entity<MCModuleValiComponent> entity)
    {
        entity.Comp.NecrosisStage = stage;
        Dirty(entity);

        if (user is null)
            return;

        _popup.PopupLocEntServer(user.Value, locId, PopupType.LargeCaution);
    }

    private void ApplyNecrosisPenalty(Entity<MCModuleValiComponent> entity, EntityUid user)
    {
        if (entity.Comp.NecrosisStartTime is null)
            return;

        var duration = _timing.CurTime - entity.Comp.NecrosisStartTime.Value;
        if (duration < entity.Comp.NecrosisThreshold)
            return;

        // TODO: Limb
        _mcDamageable.AdjustCloneLoss(user, 30 * CalculateNecrosisCount(entity));
    }

    private int CalculateNecrosisCount(Entity<MCModuleValiComponent> entity)
    {
        if (entity.Comp.NecrosisStartTime is null)
            return 0;

        var duration = _timing.CurTime - entity.Comp.NecrosisStartTime.Value;
        var seconds = duration.TotalSeconds;

        var rawValue = double.Min(seconds, 20) * 0.005d + (seconds - 20) * 0.01d;
        return int.Max(1, (int) double.Floor(rawValue));
    }

    private void BoostOn(Entity<MCModuleValiComponent> entity)
    {
        if (entity.Comp.Resource < entity.Comp.ResourceDrainAmount)
            return;

        entity.Comp.Boosted = true;
        entity.Comp.NecrosisStartTime = _timing.CurTime;
        entity.Comp.NecrosisStage = 0;
        Dirty(entity);

        ActionSetToggled<MCModuleValiBoostActionEvent>(entity, true);
    }

    private void BoostOff(Entity<MCModuleValiComponent> entity)
    {
        entity.Comp.Boosted = false;

        if (_mcArmorModule.GetUser(entity) is not { } userUid)
            return;

        ApplyNecrosisPenalty(entity, userUid);

        entity.Comp.Boosted = false;
        entity.Comp.NecrosisStartTime = null;
        entity.Comp.NecrosisStage = 0;
        Dirty(entity);

        ActionSetToggled<MCModuleValiBoostActionEvent>(userUid, false);
    }

    #region Effects

    private void ProcessBoost(Entity<MCModuleValiComponent> entity)
    {
        if (entity.Comp.Resource < entity.Comp.ResourceDrainAmount)
        {
            BoostOff(entity);
            return;
        }

        RemoveResource(entity, entity.Comp.ResourceDrainAmount);
        ProcessBoostHealingEffects(entity);
    }

    private void ProcessBoostHealingEffects(Entity<MCModuleValiComponent> entity)
    {
        if (_mcArmorModule.GetUser(entity) is not { } userUid)
            return;

        var power = entity.Comp.BoostPower;
        _mcDamageable.AdjustBruteLoss(userUid, -6 * power);
        _mcDamageable.AdjustBurnLoss(userUid, -6 * power);
    }

    #endregion
}
