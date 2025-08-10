using System.Numerics;
using Content.Shared._RMC14.Actions;
using Content.Shared.Actions;
using Content.Shared.Camera;
using Content.Shared.DoAfter;
using Content.Shared.Movement.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Zoom;

public sealed class MCXenoZoomSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly SharedContentEyeSystem _contentEye = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoZoomComponent, MCXenoZoomActionEvent>(OnAction);
        SubscribeLocalEvent<MCXenoZoomComponent, MCXenoZoomDoAfterEvent>(OnXenoZoomDoAfter);

        SubscribeLocalEvent<MCXenoZoomActiveComponent, MapInitEvent>(OnActiveInit);
        SubscribeLocalEvent<MCXenoZoomActiveComponent, ComponentRemove>(OnActiveRemove);
        SubscribeLocalEvent<MCXenoZoomActiveComponent, MoveEvent>(OnActiveMove);
        SubscribeLocalEvent<MCXenoZoomActiveComponent, GetEyeOffsetEvent>(OnActiveGetEyeOffset);
        SubscribeLocalEvent<MCXenoZoomActiveComponent, RefreshMovementSpeedModifiersEvent>(OnActiveRefreshSpeed);
    }

    private void OnAction(Entity<MCXenoZoomComponent> entity, ref MCXenoZoomActionEvent args)
    {
        if (!CanUse(entity, ref args))
            return;

        var delay = HasComp<MCXenoZoomActiveComponent>(entity) ? TimeSpan.Zero : entity.Comp.DoAfter;
        var doAfter = new DoAfterArgs(EntityManager, entity, delay, new MCXenoZoomDoAfterEvent(), entity, used: args.Action)
        {
            BreakOnMove = true,
        };

        _doAfter.TryStartDoAfter(doAfter);
    }

    private void OnXenoZoomDoAfter(Entity<MCXenoZoomComponent> entity, ref MCXenoZoomDoAfterEvent args)
    {
        if (!TryUse(entity, ref args))
            return;

        if (RemComp<MCXenoZoomActiveComponent>(entity))
            return;

        var agilityComponent = new MCXenoZoomActiveComponent
        {
            Zoom = entity.Comp.Zoom,
            Offset = Transform(args.User).LocalRotation.GetCardinalDir().ToVec() * entity.Comp.OffsetLength,
            Speed = entity.Comp.Speed,
            CanMove = entity.Comp.CanMove,
        };

        AddComp(entity, agilityComponent);
        Dirty(entity.Owner, agilityComponent);
    }

    private void OnActiveInit(Entity<MCXenoZoomActiveComponent> entity, ref MapInitEvent args)
    {
        _contentEye.SetMaxZoom(entity, entity.Comp.Zoom);
        _contentEye.SetZoom(entity, entity.Comp.Zoom);

        Refresh(entity);

        foreach (var action in _rmcActions.GetActionsWithEvent<MCXenoZoomActionEvent>(entity))
        {
            _actions.SetToggled((action, action), true);
        }
    }

    private void OnActiveRemove(Entity<MCXenoZoomActiveComponent> entity, ref ComponentRemove args)
    {
        _contentEye.ResetZoom(entity);

        Refresh(entity);

        foreach (var action in _rmcActions.GetActionsWithEvent<MCXenoZoomActionEvent>(entity))
        {
            _actions.SetToggled((action, action), false);
        }
    }

    private void OnActiveMove(Entity<MCXenoZoomActiveComponent> entity, ref MoveEvent args)
    {
        if (entity.Comp.CanMove)
            return;

        if ((args.NewPosition.Position - args.OldPosition.Position).Length() == 0)
            return;

        RemCompDeferred<MCXenoZoomActiveComponent>(entity);
    }

    private void OnActiveGetEyeOffset(Entity<MCXenoZoomActiveComponent> entity, ref GetEyeOffsetEvent args)
    {
        args.Offset += entity.Comp.Offset;
    }

    private void OnActiveRefreshSpeed(Entity<MCXenoZoomActiveComponent> entity, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(entity.Comp.Speed, entity.Comp.Speed);
    }

    private bool CanUse(Entity<MCXenoZoomComponent> entity, ref MCXenoZoomActionEvent args)
    {
        if (args.Handled)
            return false;

        if (!_rmcActions.CanUseActionPopup(entity, args.Action, entity))
            return false;

        args.Handled = true;
        return true;
    }

    private bool TryUse(Entity<MCXenoZoomComponent> entity, ref MCXenoZoomDoAfterEvent args)
    {
        if (args.Cancelled || args.Handled)
            return false;

        if (!_rmcActions.TryUseAction(entity, args.Used!.Value, entity))
            return false;

        args.Handled = true;
        return true;
    }

    private void Refresh(EntityUid uid)
    {
        _movementSpeed.RefreshMovementSpeedModifiers(uid);

        if (TryComp<EyeComponent>(uid, out var eye))
            _contentEye.UpdateEyeOffset((uid, eye));
    }
}
