using Content.Shared._MC.Mob.Stamina.Components;
using Content.Shared.Projectiles;
using Content.Shared.Throwing;
using Content.Shared.Weapons.Melee.Events;
using Content.Shared.Wieldable.Components;

namespace Content.Shared._MC.Mob.Stamina;

public sealed class MCStaminaHitSystem : EntitySystem
{
    [Dependency] private readonly MCStaminaSystem _mcStamina = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCStaminaDamageOnHitComponent, MeleeHitEvent>(OnMeleeHit);

        SubscribeLocalEvent<MCStaminaDamageOnCollideComponent, ProjectileHitEvent>(OnProjectileHit);
        SubscribeLocalEvent<MCStaminaDamageOnCollideComponent, ThrowDoHitEvent>(OnThrowHit);
    }

    private void OnMeleeHit(Entity<MCStaminaDamageOnHitComponent> entity, ref MeleeHitEvent args)
    {
        if (entity.Comp.RequiresWield && TryComp<WieldableComponent>(entity.Owner, out var wieldable) && !wieldable.Wielded)
            return;

        foreach (var targetUid in args.HitEntities)
        {
            _mcStamina.ApplyDamage(targetUid, entity.Comp.Damage);
        }
    }

    private void OnProjectileHit(Entity<MCStaminaDamageOnCollideComponent> entity, ref ProjectileHitEvent args)
    {
        _mcStamina.ApplyDamage(args.Target, entity.Comp.Damage);
    }

    private void OnThrowHit(Entity<MCStaminaDamageOnCollideComponent> entity, ref ThrowDoHitEvent args)
    {
        _mcStamina.ApplyDamage(args.Target, entity.Comp.Damage);
    }
}
