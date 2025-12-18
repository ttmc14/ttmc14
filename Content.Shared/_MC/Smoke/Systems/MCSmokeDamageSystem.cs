using Content.Shared._MC.Smoke.Components;
using Content.Shared._RMC14.Xenonids.Hive;
using Content.Shared.Damage;
using Content.Shared.Mobs.Systems;

namespace Content.Shared._MC.Smoke.Systems;

public sealed class MCSmokeDamageSystem : EntitySystem
{
    [Dependency] private readonly MobStateSystem _mobState = null!;
    [Dependency] private readonly DamageableSystem _damageable = null!;
    [Dependency] private readonly SharedXenoHiveSystem _rmcXenoHive = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCSmokeDamageComponent, MCSmokeEffectEvent>(OnEffect);
    }

    private void OnEffect(Entity<MCSmokeDamageComponent> entity, ref MCSmokeEffectEvent args)
    {
        if (_mobState.IsDead(args.TargetUid) || _rmcXenoHive.FromSameHive(entity.Owner, args.TargetUid))
            return;

        _damageable.TryChangeDamage(args.TargetUid,
            entity.Comp.Damage,
            interruptsDoAfters: false,
            origin: entity,
            tool: entity);
    }
}
