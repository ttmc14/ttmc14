using Content.Shared._RMC14.Xenonids;
using Content.Shared._RMC14.Xenonids.Energy;
using Content.Shared._RMC14.TrainingDummy;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._MC.Xeno.XenoEnergy;

public sealed class MCXenoEnergySystem : EntitySystem
{
    [Dependency] private readonly XenoSystem _xeno = default!;
    [Dependency] private readonly StandingStateSystem _stand = default!;
    [Dependency] private readonly XenoEnergySystem _xenoEnergy = default!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCAttackEnergyGainComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnMeleeHit(Entity<MCAttackEnergyGainComponent> xeno, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        var isHit = false;
        var damage = DamageSpecifier.GetPositive(args.BaseDamage + args.BonusDamage).GetTotal();
        foreach (var hit in args.HitEntities)
        {
            if (!_xeno.CanAbilityAttackTarget(xeno.Owner, hit))
                continue;

            if (HasComp<RMCTrainingDummyComponent>(hit))
                return;

            isHit = true;
            break;
        }

        if (!isHit)
            return;

        _xenoEnergy.AddEnergy(xeno, (int)Math.Floor(damage / xeno.Comp.Factor));
    }
}
