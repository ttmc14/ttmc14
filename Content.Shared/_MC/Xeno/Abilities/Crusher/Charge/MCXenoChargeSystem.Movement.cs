using Content.Shared._RMC14.Map;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Events;
using Content.Shared.Movement.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Crusher.Charge;

public sealed partial class MCXenoChargeSystem
{
    private void OnRefreshSpeed(Entity<MCXenoChargeActiveComponent> entity, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (entity.Comp.Stage == 0)
            return;

        if (!_xenoToggleChargingQuery.TryComp(entity, out var charging))
            return;

        args.ModifySpeed(1 + entity.Comp.Stage * charging.SpeedPerStage);
    }

    private void OnActiveToggleChargingMoveInput(Entity<MCXenoChargeActiveComponent> ent, ref MoveInputEvent args)
    {
        var direction = GetHeldButton(ent, args.Entity.Comp.HeldMoveButtons & MoveButtons.AnyDirection);

        if (IsSameNonDiagonalDirection(ent.Comp.Direction, direction))
            return;

        if (ent.Comp.Direction != DirectionFlag.None && TryHandlePerpendicularDeviation(ent, direction))
            return;

        ResetCharging(ent);
        ent.Comp.Direction = direction;
    }

    private void OnActiveToggleChargingMove(Entity<MCXenoChargeActiveComponent> ent, ref MoveEvent args)
    {
        if (!_xenoToggleChargingQuery.TryComp(ent, out var charging))
            return;

        if (_rmcPulling.IsBeingPulled(ent.Owner, out _))
            return;

        if (!args.OldPosition.TryDistance(EntityManager, _transform, args.NewPosition, out var distance))
            return;

        var absDistance = float.Abs(distance);
        ent.Comp.Distance += absDistance;
        ent.Comp.LastMovedAt = _timing.CurTime;
        Dirty(ent);

        if (CheckRotationReset(ent))
            return;

        if (CheckDeviationReset(ent, charging, absDistance))
            return;

        ProcessStepProgression(ent, charging);
    }

    private bool CheckRotationReset(Entity<MCXenoChargeActiveComponent> ent)
    {
        if (!_inputMoverQuery.TryComp(ent, out var mover))
            return false;

        var lastRotation = ent.Comp.LastRelativeRotation;
        ent.Comp.LastRelativeRotation = mover.RelativeRotation;

        if (ent.Comp.LastRelativeRotation == lastRotation)
            return false;

        ResetStage(ent);
        return true;
    }

    private bool CheckDeviationReset(Entity<MCXenoChargeActiveComponent> ent, MCXenoChargeComponent charging, float absDistance)
    {
        if (ent.Comp.Deviated == DirectionFlag.None)
            return false;

        ent.Comp.DeviatedDistance += absDistance;
        if (ent.Comp.DeviatedDistance >= charging.MaxDeviation)
        {
            ResetCharging(ent);
            return true;
        }
        return false;
    }

    private void ProcessStepProgression(Entity<MCXenoChargeActiveComponent> ent, MCXenoChargeComponent charging)
    {
        if (ent.Comp.Distance < charging.StepIncrement)
            return;

        ent.Comp.Steps += charging.StepIncrement;
        ent.Comp.Distance -= charging.StepIncrement;

        if (ent.Comp.Steps < charging.MinimumSteps)
            return;

        ConsumePlasmaAndProgress(ent, charging);
    }

    private void ConsumePlasmaAndProgress(Entity<MCXenoChargeActiveComponent> ent, MCXenoChargeComponent charging)
    {
        var plasmaConsume = ent.Comp.Stage * charging.SpeedPerStage * charging.PlasmaUseMultiplier;
        if (!_xenoPlasma.TryRemovePlasma(ent.Owner, plasmaConsume))
        {
            ResetCharging(ent, false);
            return;
        }

        _rmcPulling.TryStopAllPullsFromAndOn(ent);
        TryPlayEmote(ent, charging);

        ent.Comp.Stage = int.Min(charging.MaxStage, ent.Comp.Stage + 1);
        ent.Comp.SoundSteps += charging.StepIncrement;

        TryPlayChargeSound(ent, charging);

        Dirty(ent);
        _movementSpeed.RefreshMovementSpeedModifiers(ent);
    }

    private void TryPlayEmote(Entity<MCXenoChargeActiveComponent> ent, MCXenoChargeComponent charging)
    {
        if (ent.Comp.Stage == charging.MaxStage - 1 && charging.Emote is { } emote)
            _rmcEmote.TryEmoteWithChat(ent, emote, cooldown: charging.EmoteCooldown);
    }

    private void TryPlayChargeSound(Entity<MCXenoChargeActiveComponent> ent, MCXenoChargeComponent charging)
    {
        if (ent.Comp.Stage == 1 || ent.Comp.SoundSteps >= charging.SoundEvery)
        {
            ent.Comp.SoundSteps = 0;
            if (_timing.InSimulation)
                _audio.PlayPredicted(charging.Sound, ent, ent);
        }
    }

    private bool IsSameNonDiagonalDirection(DirectionFlag currentDir, DirectionFlag newDir)
    {
        return newDir != DirectionFlag.None &&
               (currentDir & newDir) == newDir &&
               (newDir & (newDir - 1)) == DirectionFlag.None;
    }

    private bool TryHandlePerpendicularDeviation(Entity<MCXenoChargeActiveComponent> ent, DirectionFlag direction)
    {
        var perpendiculars = ent.Comp.Direction.AsDir().GetPerpendiculars();
        var isPerpendicular = ent.Comp.Direction == perpendiculars.First.AsFlag() ||
                              ent.Comp.Direction == perpendiculars.Second.AsFlag();

        if (!isPerpendicular)
            return false;

        if (ent.Comp.Deviated != DirectionFlag.None && ent.Comp.Deviated != direction)
            return false;

        ent.Comp.Deviated = direction;
        return true;

    }

    private DirectionFlag GetHeldButton(EntityUid mover, MoveButtons button)
    {
        if (!TryComp<InputMoverComponent>(mover, out var moverComp))
            return DirectionFlag.None;

        var parentRotation = _moverController.GetParentGridAngle(moverComp);
        var total = _moverController.DirVecForButtons(button);
        var wishDir = _relativeMovement ? parentRotation.RotateVec(total) : total;

        return wishDir.GetDir().AsFlag();
    }
}
