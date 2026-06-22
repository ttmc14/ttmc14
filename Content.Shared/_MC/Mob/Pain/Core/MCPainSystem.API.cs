using Content.Shared._MC.Mob.Pain.Core.Components;

namespace Content.Shared._MC.Mob.Pain.Core;

public sealed partial class MCPainSystem
{
    public void AddModifier(Entity<MCPainComponent?> entity, float value)
    {
        if (!_query.Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        entity.Comp.Modifiers.Add(value);
    }

    public void SetPain(Entity<MCPainComponent?> entity, float value)
    {
        if (!_query.Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        entity.Comp.Painloss = value;
        DirtyField(entity, entity.Comp, nameof(MCPainComponent.Painloss));
        UpdateMovementSpeedModifier(entity);
    }
}
