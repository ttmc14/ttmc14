using Content.Shared._MC.Armor;
using Content.Shared._MC.Xeno.Visuals;
using Content.Shared._RMC14.Actions;
using Content.Shared.Actions;
using Content.Shared.Movement.Systems;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Agility;

public sealed class MCXenoAgilitySystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly SharedActionsSystem _actions = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly MovementSpeedModifierSystem _movementSpeed = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoAgilityComponent, MCXenoAgilityActionEvent>(OnAction);

        SubscribeLocalEvent<MCXenoAgilityActiveComponent, MapInitEvent>(OnActiveInit);
        SubscribeLocalEvent<MCXenoAgilityActiveComponent, ComponentRemove>(OnActiveRemove);
        SubscribeLocalEvent<MCXenoAgilityActiveComponent, RefreshMovementSpeedModifiersEvent>(OnActiveRefreshSpeed);
        SubscribeLocalEvent<MCXenoAgilityActiveComponent, MCArmorGetEvent>(OnActiveArmorGet);
    }

    private void OnAction(Entity<MCXenoAgilityComponent> entity, ref MCXenoAgilityActionEvent args)
    {
        if (!TryUse(entity, ref args))
            return;

        if (RemComp<MCXenoAgilityActiveComponent>(entity))
            return;

        var agilityComponent = new MCXenoAgilityActiveComponent
        {
            ArmorFlat = entity.Comp.ArmorFlat,
            SpeedModifier = entity.Comp.SpeedModifier,
        };

        AddComp(entity, agilityComponent);
        Dirty(entity.Owner, agilityComponent);
    }

    private void OnActiveInit(Entity<MCXenoAgilityActiveComponent> entity, ref MapInitEvent args)
    {
        _appearance.SetData(entity, MCXenoVisualLayers.Agility, true);
        _movementSpeed.RefreshMovementSpeedModifiers(entity);
        foreach (var action in  _rmcActions.GetActionsWithEvent<MCXenoAgilityActionEvent>(entity))
        {
            _actions.SetToggled((action, action), true);
        }
    }

    private void OnActiveRemove(Entity<MCXenoAgilityActiveComponent> entity, ref ComponentRemove args)
    {
        _appearance.SetData(entity, MCXenoVisualLayers.Agility, false);
        _movementSpeed.RefreshMovementSpeedModifiers(entity);
        foreach (var action in  _rmcActions.GetActionsWithEvent<MCXenoAgilityActionEvent>(entity))
        {
            _actions.SetToggled((action, action), false);
        }
    }

    private void OnActiveRefreshSpeed(Entity<MCXenoAgilityActiveComponent> entity, ref RefreshMovementSpeedModifiersEvent args)
    {
        args.ModifySpeed(entity.Comp.SpeedModifier);
    }

    private void OnActiveArmorGet(Entity<MCXenoAgilityActiveComponent> entity, ref MCArmorGetEvent args)
    {
        args.ArmorDefinition += entity.Comp.ArmorFlat;
    }

    private bool TryUse(Entity<MCXenoAgilityComponent> entity, ref MCXenoAgilityActionEvent args)
    {
        if (args.Handled)
            return false;

        if (_timing.ApplyingState)
            return false;

        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
            return false;

        args.Handled = true;
        return true;
    }
}
