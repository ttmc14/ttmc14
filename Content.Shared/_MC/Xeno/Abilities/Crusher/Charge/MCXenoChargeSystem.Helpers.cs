namespace Content.Shared._MC.Xeno.Abilities.Crusher.Charge;

public sealed partial class MCXenoChargeSystem
{
    private void ResetCharging(Entity<MCXenoChargeActiveComponent> entity, bool resetInput = true)
    {
        ResetStage(entity);

        entity.Comp.DeviatedDistance = 0;

        if (resetInput)
            entity.Comp.Direction = DirectionFlag.None;

        Dirty(entity);
        _movementSpeed.RefreshMovementSpeedModifiers(entity);
    }

    private void ResetStage(Entity<MCXenoChargeActiveComponent> entity)
    {
        entity.Comp.Steps = 0;
        entity.Comp.SoundSteps = 0;
        entity.Comp.Stage = 0;

        Dirty(entity);

        _movementSpeed.RefreshMovementSpeedModifiers(entity);
    }

    private void IncrementStages(Entity<MCXenoChargeActiveComponent> entity, int increment)
    {
        entity.Comp.Stage = int.Max(0, entity.Comp.Stage + increment);

        if (_xenoToggleChargingQuery.TryComp(entity, out var charging))
            entity.Comp.Stage = int.Min(charging.MaxStage, entity.Comp.Stage);

        Dirty(entity);

        _movementSpeed.RefreshMovementSpeedModifiers(entity);
    }
}
