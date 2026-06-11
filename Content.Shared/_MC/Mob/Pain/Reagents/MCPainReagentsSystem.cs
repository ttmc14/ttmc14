using Content.Shared._MC.Mob.Pain.Core;
using Content.Shared._MC.Mob.Pain.Reagents.Components;

namespace Content.Shared._MC.Mob.Pain.Reagents;

public sealed class MCPainReagentsSystem : EntitySystem
{
    [Dependency] private readonly MCPainSystem _pain = null!;

    private EntityQuery<MCPainReagentsComponent> _query;

    public override void Initialize()
    {
        _query = GetEntityQuery<MCPainReagentsComponent>();
    }

    public void EnsurePain(Entity<MCPainReagentsComponent?> entity, float amount)
    {
        if (!_query.Resolve(entity, ref entity.Comp, false))
            return;

        _pain.AddModifier(entity.Owner, amount);
    }
}
