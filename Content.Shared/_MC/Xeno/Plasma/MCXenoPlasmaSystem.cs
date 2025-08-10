using Content.Shared._RMC14.Xenonids.Plasma;
using Content.Shared.Projectiles;

namespace Content.Shared._MC.Xeno.Plasma;

public sealed class MCXenoPlasmaSystem : EntitySystem
{
    [Dependency] private readonly XenoPlasmaSystem _xenoPlasma = default!;

    private EntityQuery<XenoPlasmaComponent> _query;

    public override void Initialize()
    {
        base.Initialize();

        _query = GetEntityQuery<XenoPlasmaComponent>();

        SubscribeLocalEvent<MCXenoPlasmaDamageOnHitComponent, ProjectileHitEvent>(OnDamageHit);
    }

    private void OnDamageHit(Entity<MCXenoPlasmaDamageOnHitComponent> entity, ref ProjectileHitEvent args)
    {
        if (!_query.TryComp(args.Target, out var plasmaComponent))
            return;

        _xenoPlasma.RemovePlasma((args.Target, plasmaComponent), entity.Comp.Amount + entity.Comp.Multiplier * plasmaComponent.MaxPlasma);
    }
}
