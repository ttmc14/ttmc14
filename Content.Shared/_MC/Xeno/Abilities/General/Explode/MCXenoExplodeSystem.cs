using Content.Shared._MC.Damage;
using Content.Shared._MC.Xeno.Heal;

namespace Content.Shared._MC.Xeno.Abilities.General.Explode;

public sealed class MCXenoExplodeSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly MCDamageableSystem _mcDamageable = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoExplodeComponent, MCXenoExplodeActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoExplodeComponent> entity, ref MCXenoExplodeActionEvent args)
    {
        if (args.Handled || !RMCActions.TryUseAction(entity, args.Action, entity))
            return;

        _mcDamageable.AdjustBruteLoss(entity, 1000000);
        args.Handled = true;
    }
}
