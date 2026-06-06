using Content.Shared._MC.Knockback;

namespace Content.Shared._MC.Xeno.Abilities.Runner.Pounce.Knockback;

public sealed class MCXenoPounceKnockbackSystem : EntitySystem
{
    [Dependency] private readonly MCKnockbackSystem _knockback = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCXenoPounceKnockbackComponent, MCXenoPounceHitEvent>(OnHit);
    }

    private void OnHit(Entity<MCXenoPounceKnockbackComponent> entity, ref MCXenoPounceHitEvent args)
    {
        _knockback.KnockbackFrom(args.TargetUid, entity, entity.Comp.Entry);
    }
}
