using Content.Shared._MC.Xeno.Plasma.Components.Damage;
using Content.Shared.Projectiles;
using Content.Shared.Weapons.Melee.Events;

namespace Content.Shared._MC.Xeno.Plasma.Systems;

public sealed class MCXenoPlasmaDamageSystem : EntitySystem
{
    [Dependency] private readonly MCXenoPlasmaSystem _mcXenoPlasma = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoPlasmaDamageOnHitComponent, ProjectileHitEvent>(OnProjectileHit);
        SubscribeLocalEvent<MCXenoPlasmaDamageOnHitComponent, MeleeHitEvent>(OnMeleeHit);
    }

    private void OnProjectileHit(Entity<MCXenoPlasmaDamageOnHitComponent> entity, ref ProjectileHitEvent args)
    {
         ProcessHit(entity, args.Target);
    }

    private void OnMeleeHit(Entity<MCXenoPlasmaDamageOnHitComponent> entity, ref MeleeHitEvent args)
    {
        if (!args.IsHit)
            return;

        foreach (var hit in args.HitEntities)
        {
            ProcessHit(entity, hit);
        }
    }

    private void ProcessHit(Entity<MCXenoPlasmaDamageOnHitComponent> entity, EntityUid targetUid)
    {
        var maxPlasma = _mcXenoPlasma.GetMaxPlasma(targetUid);
        var amount = entity.Comp.Amount + entity.Comp.Multiplier * maxPlasma + _mcXenoPlasma.GetPlasmaNormalized(targetUid) * entity.Comp.MissingMultiplier;
        _mcXenoPlasma.DrainPlasma(targetUid, amount);
    }
}
