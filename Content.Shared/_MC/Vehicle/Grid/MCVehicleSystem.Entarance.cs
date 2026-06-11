using System.Numerics;
using Content.Shared._MC.Vehicle.Grid.Components;
using Robust.Shared.Map;

namespace Content.Shared._MC.Vehicle.Grid;

public sealed partial class MCVehicleSystem
{
    #region Enter

    private void TryEnter(Entity<MCVehicleComponent> entity, EntityUid user)
    {
        if (!CanEnter(entity, user))
            return;

        var difference = _transform.GetWorldPosition(user) - _transform.GetWorldPosition(entity);
        var rotation = Transform(entity).LocalRotation;
        var localDifference = (-rotation).RotateVec(difference);

        var direction = localDifference.LengthSquared() > 0.01f
            ? localDifference.GetDir()
            : Direction.Invalid;

        Enter(entity, user, direction);
    }

    private void Enter(Entity<MCVehicleComponent> entity, EntityUid user, Direction direction)
    {
        if (!GetEnterPointCoordinates(entity, direction, out var coordinates))
            return;

        _transform.SetCoordinates(user, coordinates);
    }

    private bool CanEnter(Entity<MCVehicleComponent> entity, EntityUid user)
    {
        return true;
    }

    private bool GetEnterPointCoordinates(Entity<MCVehicleComponent> entity, Direction direction, out EntityCoordinates coordinates)
    {
        coordinates = default;

        if (!GetEnterPoint(entity, direction, out var entryPointUid))
            return false;

        coordinates = Transform(entryPointUid).Coordinates;
        return true;
    }

    private bool GetEnterPoint(Entity<MCVehicleComponent> entity, Direction direction, out EntityUid entryPointUid)
    {
        entryPointUid = default;

        var query = EntityQueryEnumerator<MCVehicleGridEnterPointComponent, TransformComponent>();
        while (query.MoveNext(out var uid, out var component, out var transformComponent))
        {
            if (entity.Comp.GridUid != transformComponent.GridUid)
                continue;

            if (component.Direction != direction)
                continue;

            entryPointUid = uid;
            return true;
        }

        return false;
    }

    #endregion

    #region Leave

    private void TryLeave(Entity<MCVehicleComponent> entity, EntityUid user, Direction direction)
    {
        if (!CanLeave(entity, user, direction))
            return;

        Leave(entity, user, direction);
    }

    private bool CanLeave(Entity<MCVehicleComponent> entity, EntityUid user, Direction direction)
    {
        return true;
    }

    private void Leave(Entity<MCVehicleComponent> entity, EntityUid user, Direction direction)
    {
        var coordinates = Transform(entity).Coordinates.Offset(new Vector2(0, -2.5f));
        _transform.SetCoordinates(user, coordinates);
    }

    #endregion
}
