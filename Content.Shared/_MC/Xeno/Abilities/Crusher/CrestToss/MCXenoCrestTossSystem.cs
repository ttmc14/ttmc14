using Content.Shared._MC.Knockback;
using Content.Shared.CombatMode;
using Content.Shared.Damage;

namespace Content.Shared._MC.Xeno.Abilities.Crusher.CrestToss;

// TODO: [MC] Use MCXenoAbilitySystem<TComponent, TEvent>
public sealed class MCXenoCrestTossSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly SharedCombatModeSystem _combatMode = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly MCKnockbackSystem _mcKnockback = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoCrestTossComponent, MCXenoCrestTossActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoCrestTossComponent> entity, ref MCXenoCrestTossActionEvent args)
    {
        if (args.Handled || !TryUseAction(entity, args.Action, args.Target))
            return;

        args.Handled = true;

        _damageable.TryChangeDamage(args.Target, entity.Comp.Damage, origin: entity, tool: entity);
        _mcKnockback.KnockbackFrom(args.Target, entity, entity.Comp.Distance * GetDirection(entity), entity.Comp.Speed);

        AnimateHit(entity, args.Target);
    }

    private float GetDirection(Entity<MCXenoCrestTossComponent> entity)
    {
        return _combatMode.IsInCombatMode(entity) ? 1 : -1;
    }
}
