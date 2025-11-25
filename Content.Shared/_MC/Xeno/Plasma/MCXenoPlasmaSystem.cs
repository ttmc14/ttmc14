using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.FixedPoint;
using Content.Shared.Damage;
using Content.Shared.Mobs;
using Content.Shared.Mobs.Components;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._MC.Xeno.Plasma;

public sealed class MCXenoPlasmaSystem : EntitySystem
{
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;

    private EntityQuery<XenoPlasmaComponent> _query;
    private EntityQuery<MobStateComponent> _mobStateQuery;

    public override void Initialize()
    {
        base.Initialize();

        _query = GetEntityQuery<XenoPlasmaComponent>();
        _mobStateQuery = GetEntityQuery<MobStateComponent>();

        SubscribeLocalEvent<MCXenoPlasmaDamageOnHitComponent, ProjectileHitEvent>(OnDamageHit);
        SubscribeLocalEvent<MCXenoPlasmaDamageOnHitComponent, MeleeHitEvent>(OnDamageHitMelee);

        SubscribeLocalEvent<MobStateComponent, DamageChangedEvent>(OnDamage);
        SubscribeLocalEvent<MCXenoPlasmaOnAttackedComponent, DamageChangedEvent>(OnDamaged);
    }

    public bool TryRemovePlasma(EntityUid uid, float plasma)
    {
        if (!_query.TryComp(uid, out var plasmaComponent))
            return false;

        var previousPlasma = plasmaComponent.Plasma;
        if (previousPlasma == FixedPoint2.Zero)
            return false;

        _xenoPlasma.RemovePlasma((uid, plasmaComponent), plasma);
        return previousPlasma >= plasma;
    }

    public float GetMaxPlasma(EntityUid uid)
    {
        return !_query.TryComp(uid, out var plasmaComponent) ? 0 : plasmaComponent.MaxPlasma;
    }

    private void OnDamageHit(Entity<MCXenoPlasmaDamageOnHitComponent> entity, ref ProjectileHitEvent args)
    {
        if (!_query.TryComp(args.Target, out var plasmaComponent))
            return;

        var baseRemoval = entity.Comp.Amount + (FixedPoint2) (entity.Comp.Multiplier * plasmaComponent.MaxPlasma);

        float missingFrac = 0f;
        if (plasmaComponent.MaxPlasma > 0)
        {
            var current = (float) plasmaComponent.Plasma;
            var max = (float) plasmaComponent.MaxPlasma;
            missingFrac = (max - current) / max;
            if (missingFrac < 0f)
                missingFrac = 0f;
            else if (missingFrac > 1f)
                missingFrac = 1f;
        }

        var extraRemoval = (FixedPoint2) (missingFrac * entity.Comp.MissingMultiplier * plasmaComponent.MaxPlasma);

        _xenoPlasma.RemovePlasma((args.Target, plasmaComponent), baseRemoval + extraRemoval);
    }

    private void OnDamageHitMelee(Entity<MCXenoPlasmaDamageOnHitComponent> entity, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        foreach (var hit in args.HitEntities)
        {
            if (!_query.TryComp(hit, out var plasmaComponent))
                continue;

            var baseRemoval = entity.Comp.Amount + (FixedPoint2) (entity.Comp.Multiplier * plasmaComponent.MaxPlasma);

            float missingFrac = 0f;
            if (plasmaComponent.MaxPlasma > 0)
            {
                var current = (float) plasmaComponent.Plasma;
                var max = (float) plasmaComponent.MaxPlasma;
                missingFrac = (max - current) / max;
                if (missingFrac < 0f)
                    missingFrac = 0f;
                else if (missingFrac > 1f)
                    missingFrac = 1f;
            }

            var extraRemoval = (FixedPoint2) (missingFrac * entity.Comp.MissingMultiplier * plasmaComponent.MaxPlasma);

            _xenoPlasma.RemovePlasma((hit, plasmaComponent), baseRemoval + extraRemoval);
        }
    }

    private void OnDamage(Entity<MobStateComponent> ent, ref DamageChangedEvent args)
    {
        if (ent.Comp.CurrentState == MobState.Dead)
            return;

        if (args.Origin is not {} origin)
            return;

        if (!TryComp<MCXenoPlasmaOnAttackComponent>(origin, out var plasmaOnAttackComponent))
            return;

        _xenoPlasma.RegenPlasma(origin, args.DamageDelta?.GetTotal() ?? FixedPoint2.Zero * plasmaOnAttackComponent.Multiplier);
    }

    private void OnDamaged(Entity<MCXenoPlasmaOnAttackedComponent> ent, ref DamageChangedEvent args)
    {
        if (!args.DamageIncreased || args.DamageDelta is null)
            return;

        _xenoPlasma.RegenPlasma(ent.Owner, args.DamageDelta.GetTotal() * ent.Comp.Multiplier);
    }
}
