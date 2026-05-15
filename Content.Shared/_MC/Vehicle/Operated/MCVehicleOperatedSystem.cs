using Content.Shared._MC.Vehicle.Operated.Components;
using Content.Shared.ActionBlocker;
using Content.Shared.Movement.Systems;
using Content.Shared.Projectiles;
using Robust.Shared.Physics.Events;

namespace Content.Shared._MC.Vehicle.Operated;

public sealed partial class MCVehicleOperatedSystem : EntitySystem
{
    [Dependency] private readonly SharedMoverController _mover = null!;
    [Dependency] private readonly ActionBlockerSystem _actionBlocker = null!;

    public override void Initialize()
    {
        InitializeEnter();
        InitializeOperator();

        SubscribeLocalEvent<MCVehicleOperatedComponent, PreventCollideEvent>(OnPreventCollide);
    }

    private void OnPreventCollide(Entity<MCVehicleOperatedComponent> entity, ref PreventCollideEvent args)
    {
        if (!TryComp<ProjectileComponent>(args.OtherEntity, out var projectileComponent))
            return;

        if (projectileComponent.Shooter != entity.Comp.Operator)
            return;

        args.Cancelled = true;
    }
}
