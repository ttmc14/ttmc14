using Content.Shared._MC.Smoke.Components;
using Content.Shared.Damage;

namespace Content.Shared._MC.Smoke.Systems;

public sealed class MCSmokeDamageSystem : EntitySystem
{
    [Dependency] private readonly DamageableSystem _damageable = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCSmokeDamageComponent, MCSmokeEffectEvent>(OnEffect);
    }

    private void OnEffect(Entity<MCSmokeDamageComponent> entity, ref MCSmokeEffectEvent args)
    {
        _damageable.TryChangeDamage(args.TargetUid,
            entity.Comp.Damage,
            interruptsDoAfters: false,
            origin: entity,
            tool: entity);
    }
}
