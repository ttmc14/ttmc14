using Content.Shared._MC.Mob.Movement;
using Content.Shared._MC.Mob.Stamina.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;
using Robust.Shared.Network;

namespace Content.Shared._MC.Mob.Stamina;

public sealed class MCStaminaActiveSystem : EntitySystem
{
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly SharedMoverController _moverController = null!;
    [Dependency] private readonly MCStaminaSystem _mcStamina = null!;

    private EntityQuery<InputMoverComponent> _inputMoverQuery;

    public override void Initialize()
    {
        base.Initialize();

        _inputMoverQuery = GetEntityQuery<InputMoverComponent>();

        SubscribeLocalEvent<MCStaminaActiveComponent, MCMobStepEvent>(OnMobStep);
    }

    private void OnMobStep(Entity<MCStaminaActiveComponent> entity, ref MCMobStepEvent args)
    {
        // TODO: Predict
        if (_net.IsClient)
            return;

        if (!_inputMoverQuery.TryComp(entity.Owner, out var inputMoverComponent))
            return;

        if (!args.Sprinting)
            return;

        if (_mcStamina.GetLoss(entity.Owner) > entity.Comp.ReviveThreshold)
        {
            _moverController.SetSprinting((entity.Owner, inputMoverComponent), 0, true);
            return;
        }

        _mcStamina.ApplyDamage(entity.Owner, entity.Comp.BaseStepCost * args.FrameTime);
    }
}
