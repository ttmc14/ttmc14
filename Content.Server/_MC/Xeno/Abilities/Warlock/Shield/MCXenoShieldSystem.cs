using Content.Server.Explosion.EntitySystems;
using Content.Shared._MC.Xeno.Abilities.Warlock.Shield;
using Content.Shared._MC.Xeno.Abilities.Warlock.Shield.Components;

namespace Content.Server._MC.Xeno.Abilities.Warlock.Shield;

public sealed class MCXenoShieldSystem : MCSharedXenoShieldSystem
{
    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCXenoShieldFrozenProjectileComponent, BeforeTriggerEvent>(OnProjectileTrigger);
    }

    private void OnProjectileTrigger(Entity<MCXenoShieldFrozenProjectileComponent> entity, ref BeforeTriggerEvent args)
    {
        args.Cancelled = true;
    }
}
