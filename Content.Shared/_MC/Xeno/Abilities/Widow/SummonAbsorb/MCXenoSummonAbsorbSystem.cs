using Content.Shared._MC.Damage;
using Content.Shared._MC.Xeno.Abilities.Widow.Summon;
using Content.Shared._MC.Xeno.Heal;
using Content.Shared._MC.Xeno.Plasma.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Widow.SummonAbsorb;

public sealed class MCXenoSummonAbsorbSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly MCDamageableSystem _mcDamageable = null!;
    [Dependency] private readonly MCXenoHealSystem _mcXenoHeal = null!;
    [Dependency] private readonly MCXenoPlasmaSystem _mcXenoPlasma = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoSummonAbsorbComponent, MCXenoSummonAbsorbActionEvent>(OnAction);
    }

    private void OnAction(Entity<MCXenoSummonAbsorbComponent> entity, ref MCXenoSummonAbsorbActionEvent args)
    {
        if (args.Handled)
            return;

        if (!IsXeno(args.Target))
            return;

        if (!HasComp<MCXenoSummonedComponent>(args.Target))
            return;

        if (!TryUseAction(entity, args.Action))
            return;

        _mcXenoHeal.Heal(entity, _mcXenoHeal.GetHealth(args.Target));
        _mcXenoPlasma.RegenPlasma(entity, _mcXenoPlasma.GetPlasma(args.Target));

        _mcDamageable.AdjustBruteLoss(args.Target, 9999);

        args.Handled = true;
    }
}
