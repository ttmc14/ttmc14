using Content.Shared.Atmos.Components;
using Content.Shared.Damage;

namespace Content.Shared._MC.Xeno.Abilities.Pounce.Firecharge;

public sealed class MCXenoPounceFireChargeSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoPounceFireChargeComponent, MCXenoPounceHitEvent>(OnHit);
    }

    private void OnHit(Entity<MCXenoPounceFireChargeComponent> entity, ref MCXenoPounceHitEvent args)
    {
        if (!TryComp<FlammableComponent>(args.TargetUid, out var fireStacksComp))
            return;

        _damageable.TryChangeDamage(args.TargetUid, fireStacksComp.FireStacks * entity.Comp.DamagePerFireStack, origin: entity, tool: entity);
        fireStacksComp.FireStacks = 0;

        Dirty(args.TargetUid, fireStacksComp);
    }
}
