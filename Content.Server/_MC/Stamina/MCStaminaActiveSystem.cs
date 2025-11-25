using Content.Shared._MC;
using Content.Shared._MC.Stamina;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Configuration;
using Robust.Shared.Physics.Components;

namespace Content.Server._MC.Stamina;

public sealed class MCStaminaActiveSystem : EntitySystem
{
    [Dependency] private readonly IConfigurationManager _configuration = default!;

    [Dependency] private readonly SharedMoverController _moverController = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeedModifier = default!;

    [Dependency] private readonly MCStaminaSystem _mcStamina = default!;

    private bool _enabled = true;

    public override void Initialize()
    {
        _configuration.OnValueChanged(MCConfigVars.MCStaminaDamageOnRun, value => _enabled = value, invokeImmediately: true);

        SubscribeLocalEvent<MCStaminaActiveComponent, RefreshMovementSpeedModifiersEvent>(OnRefresh);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        if (!_enabled)
            return;

        var query = EntityQueryEnumerator<MCStaminaComponent, MCStaminaActiveComponent, PhysicsComponent, InputMoverComponent>();
        while (query.MoveNext(out var uid, out var stamina, out var active, out var phys, out var input))
        {
            if (stamina.Current <= 0 && !active.ZeroSprintLock)
            {
                active.ZeroSprintLock = true;
                _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
                _moverController.SetSprinting((uid, input), 0, true);
            }

            if (active.ZeroSprintLock && stamina.Current >= 50)
            {
                if (input.Sprinting)
                {
                    _moverController.SetSprinting((uid, input), 0, true);
                }
                else
                {
                    active.ZeroSprintLock = false;
                    _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
                }
            }

            if (active.ZeroSprintLock)
            {
                _moverController.SetSprinting((uid, input), 0, true);
                continue;
            }

            if (input.Sprinting && !active.Slowed && phys.LinearVelocity.Length() > 0.1f && stamina.Current > 0)
                _mcStamina.Damage((uid, stamina), active.RunStaminaDamage, false);

            if (stamina.Current >= active.SlowThreshold && !active.Slowed)
            {
                active.Slowed = true;
                active.Change = true;
                _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
                continue;
            }

            if (stamina.Current > active.ReviveStaminaLevel || !active.Slowed)
                continue;

            active.Slowed = false;
            active.Change = true;
            _movementSpeedModifier.RefreshMovementSpeedModifiers(uid);
        }
    }

    private void OnRefresh(Entity<MCStaminaActiveComponent> ent, ref RefreshMovementSpeedModifiersEvent args)
    {
        if (!_enabled)
            return;

        if (ent.Comp.ZeroSprintLock)
        {
            args.ModifySpeed(args.WalkSpeedModifier, args.WalkSpeedModifier);
            return;
        }

        if (!ent.Comp.Change)
            return;

        args.ModifySpeed(args.WalkSpeedModifier, args.SprintSpeedModifier);
    }
}
