using Content.Shared.Projectiles;
using Content.Shared.StatusEffectNew;

namespace Content.Shared._MC.Weapons.Projectiles.StatusEffects;

public sealed class MCProjectileStatusEffectsSystem : EntitySystem
{
    [Dependency] private readonly SharedStatusEffectsSystem _statusEffects = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCProjectileStatusEffectsOnHitComponent, ProjectileHitEvent>(OnHit);
    }

    private void OnHit(Entity<MCProjectileStatusEffectsOnHitComponent> entity, ref ProjectileHitEvent args)
    {
        foreach (var entry in entity.Comp.StatusEffects)
        {
            _statusEffects.TrySetStatusEffectDuration(args.Target, entry.EffectId, entry.Duration);
        }
    }
}
