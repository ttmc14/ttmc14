using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Robust.Shared.Map;

namespace Content.Shared._MC.Weapon;

public sealed class MCZLevelShootHelperSystem : EntitySystem
{
    [Dependency] private readonly CESharedZLevelsSystem _zLevels = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    public void ApplyZPhysics(EntityUid shooter, List<EntityUid> projectiles, EntityCoordinates targetCoords, float speed)
    {
        if (speed <= 0f)
            return;

        if (!_zLevels.IsBelowAtCoordinates(targetCoords, out _))
            return;

        var shooterPos = _transform.GetMapCoordinates(shooter);
        var targetPos = _transform.ToMapCoordinates(targetCoords);
        var distance = (targetPos.Position - shooterPos.Position).Length();

        if (distance <= 0f)
            return;

        var timeToReach = distance / speed;

        foreach (var projectile in projectiles)
        {
            var zPhys = EnsureComp<CEZPhysicsComponent>(projectile);

            // Математика спуска
            zPhys.Velocity = -1.25f / timeToReach;
            zPhys.GravityMultiplier = 0;
            zPhys.Bounciness = 0;
            zPhys.Fallable = false;
            zPhys.CachedStickyGround = true;
            zPhys.LocalPosition = 0.25f;
        }
    }
}
