using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared._MC.Stun;

namespace Content.Shared._MC.ZLevels;

public sealed class MCZLevelFallStunSystem : EntitySystem
{
    [Dependency] private readonly MCStunSystem _mcStun = null!;

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCZLevelFallStunComponent, CEZLevelHitEvent>(OnHit);
    }

    private void OnHit(Entity<MCZLevelFallStunComponent> entity, ref CEZLevelHitEvent args)
    {
        _mcStun.Slowdown(entity, entity.Comp.SlowTime * args.ImpactPower);
        _mcStun.Paralyze(entity, entity.Comp.StunTime * args.ImpactPower);
    }
}
