using Content.Shared._MC.Xeno.Heal;
using Content.Shared._MC.Xeno.Plasma.Systems;

namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce.Heal;

public sealed class MCXenoPounceHealSystem : MCXenoAbilitySystem
{
    [Dependency] private readonly MCXenoHealSystem _mcXenoHeal = null!;
    [Dependency] private readonly MCXenoPlasmaSystem _mcXenoPlasma = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoPounceHealComponent, MCXenoPounceHitEvent>(OnHit);
    }

    private void OnHit(Entity<MCXenoPounceHealComponent> entity, ref MCXenoPounceHitEvent args)
    {
        if (!args.First)
            return;

        _mcXenoHeal.Heal(entity, entity.Comp.AdjustHealth);
        _mcXenoPlasma.RegenPlasma(entity, entity.Comp.AdjustPlasma);
    }
}
