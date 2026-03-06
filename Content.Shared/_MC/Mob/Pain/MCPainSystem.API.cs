using Content.Shared.Damage;
using Content.Shared.Movement.Systems;

namespace Content.Shared._MC.Mob.Pain;

public sealed partial class MCPainSystem
{
    public void SetPain(Entity<MCPainComponent?> entity, float value)
    {
        if (!_painQuery.Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        entity.Comp.Painloss = value;
        DirtyField(entity, entity.Comp, nameof(MCPainComponent.Painloss));
        UpdateMovementSpeedModifier(entity);
    }
}
