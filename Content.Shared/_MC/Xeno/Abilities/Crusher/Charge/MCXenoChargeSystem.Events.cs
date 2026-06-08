using Content.Shared.Mobs;
using Content.Shared.Movement.Components;

namespace Content.Shared._MC.Xeno.Abilities.Crusher.Charge;

public sealed partial class MCXenoChargeSystem
{
    private void OnAction(Entity<MCXenoChargeComponent> entity, ref MCXenoChargeActionEvent args)
    {
        if (_timing.ApplyingState)
            return;

        if (RemComp<MCXenoChargeActiveComponent>(entity))
            return;

        if (!TryComp<InputMoverComponent>(entity, out var mover))
            return;

        var direction = GetHeldButton(entity, mover.HeldMoveButtons);

        var active = new MCXenoChargeActiveComponent();
        AddComp(entity, active, true);

        if ((direction & (direction - 1)) == DirectionFlag.None)
            active.Direction = direction;

        Dirty(entity, active);
    }

    private void OnActiveInit(Entity<MCXenoChargeActiveComponent> entity, ref MapInitEvent args)
    {
        _movementSpeed.RefreshMovementSpeedModifiers(entity);

        foreach (var action in _rmcActions.GetActionsWithEvent<MCXenoChargeActionEvent>(entity))
        {
            _actions.SetToggled((action, action), true);
        }
    }

    private void OnActiveRemove(Entity<MCXenoChargeActiveComponent> entity, ref ComponentRemove args)
    {
        _movementSpeed.RefreshMovementSpeedModifiers(entity);

        foreach (var action in _rmcActions.GetActionsWithEvent<MCXenoChargeActionEvent>(entity))
        {
            _actions.SetToggled((action, action), false);
        }
    }

    private void OnActiveToggleChargingMobStateChanged(Entity<MCXenoChargeActiveComponent> ent, ref MobStateChangedEvent args)
    {
        if (args.NewMobState == MobState.Alive)
            return;

        ResetCharging(ent);

        if (_timing.ApplyingState)
            return;

        RemComp<MCXenoChargeActiveComponent>(ent);
    }
}
