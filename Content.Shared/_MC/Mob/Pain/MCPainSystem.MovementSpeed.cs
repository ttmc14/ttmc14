using Content.Shared.Movement.Systems;

namespace Content.Shared._MC.Mob.Pain;

public sealed partial class MCPainSystem
{
    private readonly MovementSpeedModifierSystem _movementSpeedModifiers = null!;

    private void UpdateMovementSpeedModifier(Entity<MCPainComponent?> entity)
    {
        if (!_painQuery.Resolve(entity, ref entity.Comp, logMissing: false))
            return;

        var painloss = entity.Comp.Painloss;

        var speedModifier = entity.Comp.MovementSpeedModifier;
        foreach (var (pain, modifier) in entity.Comp.MovementSpeedModifiers)
        {
            if (painloss <= pain)
                continue;

            speedModifier = modifier;
        }

        if (float.Abs(entity.Comp.MovementSpeedModifier - speedModifier) < 1e-5f)
            return;

        entity.Comp.MovementSpeedModifier = speedModifier;
        DirtyField(entity, entity.Comp, nameof(entity.Comp.MovementSpeedModifier));

        _movementSpeedModifiers.RefreshMovementSpeedModifiers(entity);
    }
}
