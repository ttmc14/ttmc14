using Content.Shared._RMC14.Slow;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Projectiles;
using Robust.Shared.Timing;

namespace Content.Shared._MC.Xeno.Abilities.Sentinel.ToxicStacks;

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

            Add((uid, component), -component.Decay);
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
        Add(args.Target, entity.Comp.Amount);
    }

    public bool HasImmune(EntityUid uid)
    {
        return !HasComp<MCXenoToxicStacksComponent>(uid);
    }

    public int Get(EntityUid uid)
    {
        return CompOrNull<MCXenoToxicStacksComponent>(uid)?.Count ?? 0;
    }

    public int GetMax(EntityUid uid)
    {
        return CompOrNull<MCXenoToxicStacksComponent>(uid)?.Count ?? 0;
    }

    public void Add(Entity<MCXenoToxicStacksComponent?> entity, float count)
    {
        TryAdd(entity, (int) float.Round(count));
    }

    public void Add(Entity<MCXenoToxicStacksComponent?> entity, int count)
    {
        TryAdd(entity, count);
    }

    public bool TryAdd(Entity<MCXenoToxicStacksComponent?> entity, int count)
    {
        if (!Resolve(entity, ref entity.Comp, logMissing: false))
            return false;

        Set(entity, entity.Comp.Count + count);
        return true;
    }

    public void Remove(Entity<MCXenoToxicStacksComponent?> entity, float count)
    {
        Remove(entity, (int) float.Round(count));
    }

    public void Remove(Entity<MCXenoToxicStacksComponent?> entity, int count)
    {
        if (!Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        Set(entity, entity.Comp.Count - count);
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
