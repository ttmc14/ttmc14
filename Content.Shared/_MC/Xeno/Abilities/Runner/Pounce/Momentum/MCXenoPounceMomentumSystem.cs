using Content.Shared._MC.Xeno.Abilities.Warrior.Momentum;

namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce.Momentum;

public sealed class MCXenoPounceMomentumSystem : EntitySystem
{
    [Dependency] private readonly MCXenoMomentumSystem _momentum = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoPounceMomentumComponent, MCXenoPounceHitEvent>(OnHit);
    }

    private void OnHit(Entity<MCXenoPounceMomentumComponent> entity, ref MCXenoPounceHitEvent args)
    {
        _momentum.AddStacks(entity.Owner, entity.Comp.Gain);
    }
}
