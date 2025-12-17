using Content.Shared._RMC14.Actions;
using Content.Shared._RMC14.Pulling;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.CombatMode;
using Content.Shared.Damage;
using Content.Shared.Mobs.Components;
using Content.Shared.Mobs.Systems;
using Content.Shared.Throwing;

namespace Content.Shared._MC.Xeno.Abilities.CrestToss;

public sealed class MCXenoCrestTossSystem : EntitySystem
{
    [Dependency] private readonly SharedCombatModeSystem _combatMode = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly RMCActionsSystem _rmcActions = default!;
    [Dependency] private readonly SharedXenoHiveSystem _xenoHive = default!;
    [Dependency] private readonly MobStateSystem _mobState = default!;
    [Dependency] private readonly ThrowingSystem _throwing = default!;
    [Dependency] private readonly RMCPullingSystem _rmcPulling = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoCrestTossComponent, MCXenoCrestTossActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoCrestTossComponent> entity, ref MCXenoCrestTossActionEvent args)
    {
        if (args.Handled)
            return;

        if (!HasComp<MobStateComponent>(args.Target))
            return;

        if (_mobState.IsDead(args.Target))
            return;

        if (_xenoHive.FromSameHive(entity.Owner, args.Target))
            return;

        if (!_rmcActions.TryUseAction(entity, args.Action, entity))
            return;

        args.Handled = true;

        var origin = _transform.GetMapCoordinates(entity);
        var delta = (_transform.GetMapCoordinates(args.Target).Position - origin.Position).Normalized() * entity.Comp.Distance;

        if (!_combatMode.IsInCombatMode(entity))
            delta *= -1;

        _damageable.TryChangeDamage(args.Target, entity.Comp.Damage, origin: entity, tool: entity);
        _rmcPulling.TryStopAllPullsFromAndOn(args.Target);
        _throwing.TryThrow(args.Target, delta, entity.Comp.Speed);
    }
}
