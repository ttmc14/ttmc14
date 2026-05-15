using Content.Shared._MC.Vehicle.Operated.Components;
using Content.Shared._MC.Vehicle.Operated.Events;
using Content.Shared.Movement.Components;
using JetBrains.Annotations;

namespace Content.Shared._MC.Vehicle.Operated;

public sealed partial class MCVehicleOperatedSystem
{
    [PublicAPI]
    public bool TryGetOperator(Entity<MCVehicleOperatedComponent?> entity, out Entity<MCVehicleOperatorComponent> operatorEntity)
    {
        operatorEntity = default;

        if (!Resolve(entity, ref entity.Comp))
            return false;

        if (entity.Comp.Operator is not { } operatorUid)
            return false;

        if (!TryComp<MCVehicleOperatorComponent>(operatorUid, out var operatorComponent))
            return false;

        operatorEntity = (operatorUid, operatorComponent);
        return true;
    }

    [PublicAPI]
    public bool TryRemoveOperator(Entity<MCVehicleOperatorComponent?> operatorEntity)
    {
        if (!Resolve(operatorEntity, ref operatorEntity.Comp, false))
            return true;

        return !TryComp<MCVehicleOperatedComponent>(operatorEntity.Comp.Vehicle, out var vehicle) ||
               TrySetOperator((operatorEntity.Comp.Vehicle.Value, vehicle), null, removeExisting: true);
    }

    [PublicAPI]
    public bool TryRemoveOperator(Entity<MCVehicleOperatedComponent> entity)
    {
        return TrySetOperator(entity, null, removeExisting: true);
    }

    [PublicAPI]
    public bool TrySetOperator(Entity<MCVehicleOperatedComponent> entity, EntityUid? uid, bool removeExisting = true)
    {
        if (!ValidateOperatorChange(entity, uid, removeExisting))
            return false;

        var operatorOldUid = entity.Comp.Operator;

        if (entity.Comp.Operator is { } currentOperator && TryComp<MCVehicleOperatorComponent>(currentOperator, out var currentOperatorComponent))
        {
            var exitEvent = new MCVehicleOperatedRemovedEvent(entity, currentOperator);
            RaiseLocalEvent(currentOperator, ref exitEvent);

            currentOperatorComponent.Vehicle = null;

            RemCompDeferred<MCVehicleOperatorComponent>(currentOperator);
            RemCompDeferred<RelayInputMoverComponent>(currentOperator);
        }

        entity.Comp.Operator = uid;

        if (uid is { } operatorNewUid)
        {
            var vehicleOperator = EnsureComp<MCVehicleOperatorComponent>(operatorNewUid);

            vehicleOperator.Vehicle = entity;
            Dirty(operatorNewUid, vehicleOperator);

            _mover.SetRelay(operatorNewUid, entity);

            var enterEvent = new MCVehicleOperatedAddedEvent(entity, operatorNewUid);
            RaiseLocalEvent(uid.Value, ref enterEvent);
        }
        else
        {
            RemCompDeferred<MovementRelayTargetComponent>(entity);
        }

        _actionBlocker.UpdateCanMove(entity);
        RaiseChangedEvent(entity, uid, operatorOldUid);

        return true;
    }

    private bool ValidateOperatorChange(Entity<MCVehicleOperatedComponent> entity, EntityUid? uid, bool removeExisting)
    {
        if (entity.Comp.Operator is null && uid is null)
            return false;

        if (TryComp<MCVehicleOperatorComponent>(uid, out var operatorComponent))
            return operatorComponent.Vehicle != entity.Owner;

        if (!removeExisting && entity.Comp.Operator is not null)
            return false;

        return true;
    }

    private void RaiseChangedEvent(Entity<MCVehicleOperatedComponent> entity, EntityUid? newOperatorUid, EntityUid? oldOperatorUid)
    {
        var ev = new MCVehicleOperatedChangedEvent(newOperatorUid, oldOperatorUid);
        RaiseLocalEvent(entity, ref ev);
        Dirty(entity);
    }
}
