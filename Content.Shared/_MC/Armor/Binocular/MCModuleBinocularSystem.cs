using System.Numerics;
using Content.Shared._MC.Armor.Modules.Components;
using Content.Shared._MC.Armor.Modules.Events;
using Content.Shared._MC.Armor.Modules.Systems;
using Content.Shared._RMC14.Actions;
using Content.Shared.Actions;
using Content.Shared.Camera;
using Content.Shared.Movement.Systems;

namespace Content.Shared._MC.Armor.Binocular;

public sealed class MCModuleBinocularSystem : EntitySystem
{
    [Dependency] private readonly SharedActionsSystem _actions = null!;
    [Dependency] private readonly SharedContentEyeSystem _contentEye = null!;
    [Dependency] private readonly SharedRMCActionsSystem _rmcActions = null!;
    [Dependency] private readonly MCArmorModuleRelaySystem _mcArmorModuleRelay = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCArmorModularClothingComponent, MCModuleBinocularActionEvent>(_mcArmorModuleRelay.RelayEvent);

        SubscribeLocalEvent<MCModuleBinocularComponent, MCArmorModuleRelayedEvent<GetItemActionsEvent>>(OnGetAction);
        SubscribeLocalEvent<MCModuleBinocularComponent, MCArmorModuleRelayedEvent<MCModuleBinocularActionEvent>>(OnAction);
        SubscribeLocalEvent<MCModuleBinocularComponent, MCArmorModuleDetachedEvent>(OnDeattached);

        SubscribeLocalEvent<MCModuleBinocularActiveComponent, MapInitEvent>(OnActiveInit);
        SubscribeLocalEvent<MCModuleBinocularActiveComponent, ComponentRemove>(OnActiveRemove);
        SubscribeLocalEvent<MCModuleBinocularActiveComponent, MoveEvent>(OnActiveMove);
        SubscribeLocalEvent<MCModuleBinocularActiveComponent, GetEyeOffsetEvent>(OnActiveGetEyeOffset);
    }

    private void OnGetAction(Entity<MCModuleBinocularComponent> entity, ref MCArmorModuleRelayedEvent<GetItemActionsEvent> args)
    {
        args.Args.AddAction(ref entity.Comp.ActionUid, entity.Comp.ActionId);
        Dirty(entity);
    }

    private void OnAction(Entity<MCModuleBinocularComponent> entity, ref MCArmorModuleRelayedEvent<MCModuleBinocularActionEvent> relayedArgs)
    {
        var args = relayedArgs.Args;
        if (args.Handled)
            return;

        args.Handled = true;

        if (GetUser(entity) is not { } user)
            return;

        if (RemCompDeferred<MCModuleBinocularActiveComponent>(user))
            return;

        var agilityComponent = new MCModuleBinocularActiveComponent
        {
            Zoom = entity.Comp.Zoom,
            Offset = Transform(user).LocalRotation.GetCardinalDir().ToVec() * entity.Comp.OffsetLength,
            CanMove = entity.Comp.CanMove,
        };

        AddComp(user, agilityComponent);
        Dirty(user, agilityComponent);
    }

    private void OnDeattached(Entity<MCModuleBinocularComponent> entity, ref MCArmorModuleDetachedEvent args)
    {
        if (args.User is not { } user)
            return;

        RemCompDeferred<MCModuleBinocularActiveComponent>(user);
    }

    private void OnActiveInit(Entity<MCModuleBinocularActiveComponent> entity, ref MapInitEvent args)
    {
        _contentEye.SetMaxZoom(entity, entity.Comp.Zoom);
        _contentEye.SetZoom(entity, entity.Comp.Zoom);

        Refresh(entity);
        SetToggled(entity, true);
    }

    private void OnActiveRemove(Entity<MCModuleBinocularActiveComponent> entity, ref ComponentRemove args)
    {
        _contentEye.SetMaxZoom(entity, Vector2.One);
        _contentEye.SetZoom(entity, Vector2.One);

        _contentEye.ResetZoom(entity);

        Refresh(entity);
        SetToggled(entity, false);
    }

    private void OnActiveMove(Entity<MCModuleBinocularActiveComponent> entity, ref MoveEvent args)
    {
        if (entity.Comp.CanMove)
            return;

        if ((args.NewPosition.Position - args.OldPosition.Position).Length() == 0)
            return;

        RemCompDeferred<MCModuleBinocularActiveComponent>(entity);
    }

    private static void OnActiveGetEyeOffset(Entity<MCModuleBinocularActiveComponent> entity, ref GetEyeOffsetEvent args)
    {
        args.Offset += entity.Comp.Offset;
    }

    private void Refresh(EntityUid uid)
    {
        if (TryComp<EyeComponent>(uid, out var eye))
            _contentEye.UpdateEyeOffset((uid, eye));
    }

    private void SetToggled(Entity<MCModuleBinocularActiveComponent> entity, bool value)
    {
        foreach (var action in _rmcActions.GetActionsWithEvent<MCModuleBinocularActionEvent>(entity))
        {
            _actions.SetToggled((action, action), value);
        }
    }

    private EntityUid? GetUser(EntityUid uid)
    {
        return !TryComp<MCArmorModularClothingComponent>(Transform(uid).ParentUid, out var containerComponent) ? null : containerComponent.CurrentUser;
    }
}
