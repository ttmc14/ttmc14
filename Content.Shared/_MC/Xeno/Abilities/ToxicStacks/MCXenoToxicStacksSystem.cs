using Content.Shared._RMC14.Slow;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Projectiles;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.ToxicStacks;

public sealed class MCXenoToxicStacksSystem : EntitySystem
{
    [Dependency] private readonly SharedAppearanceSystem _appearance = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly DamageableSystem _damageable = default!;
    [Dependency] private readonly RMCSlowSystem _rmcSlow = default!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoToxicStacksComponent, MobStateChangedEvent>(OnMobChangedState);
        SubscribeLocalEvent<MCXenoToxicStacksOnHitComponent, ProjectileHitEvent>(OnProjectileHit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        var query = EntityQueryEnumerator<MCXenoToxicStacksComponent>();
        while (query.MoveNext(out var uid, out var component))
        {
            if (_timing.CurTime < component.NextTick)
                continue;

            component.NextTick = _timing.CurTime + component.TickInterval;
            Dirty(uid, component);

            if (component.Count == 0)
                continue;

            _damageable.TryChangeDamage(uid, component.BaseDamage + component.StacksDamage * Math.Round(component.Count / 10f), ignoreResistances: true);

            if (component.Count >= 20)
                _rmcSlow.TrySlowdown(uid, TimeSpan.FromSeconds(1));

            TryAdd((uid, component), -component.Decay);
        }
    }

    private void OnMobChangedState(Entity<MCXenoToxicStacksComponent> entity, ref MobStateChangedEvent args)
    {
        if (args.NewMobState != MobState.Dead)
            return;

        Set((entity, entity), 0);
    }

    private void OnProjectileHit(Entity<MCXenoToxicStacksOnHitComponent> entity, ref ProjectileHitEvent args)
    {
        TryAdd(args.Target, entity.Comp.Amount);
    }

    public bool TryAdd(Entity<MCXenoToxicStacksComponent?> entity, int count)
    {
        if (!Resolve(entity, ref entity.Comp, logMissing: false))
            return false;

        Set(entity, entity.Comp.Count + count);
        return true;
    }

    public void Set(Entity<MCXenoToxicStacksComponent?> entity, int count)
    {
        if (!Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        entity.Comp.Count = Math.Clamp(count, 0, entity.Comp.Max);
        Dirty(entity);

        _appearance.SetData(entity, MCXenoToxicStacksVisuals.Visuals, entity.Comp.Count);
    }
}
