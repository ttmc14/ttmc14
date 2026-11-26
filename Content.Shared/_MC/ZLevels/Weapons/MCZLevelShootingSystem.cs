using System.Numerics;
using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared._MC.ZLevels.Mathematics;
using Content.Shared._MC.ZLevels.Views;
using Content.Shared._MC.ZLevels.Weapons.Components;
using Robust.Shared.Map;
using Robust.Shared.Timing;

namespace Content.Shared._MC.ZLevels.Weapons;

public sealed class MCZLevelShootingSystem : EntitySystem
{
    [Dependency] private readonly IGameTiming _timing = null!;

    [Dependency] private readonly CESharedZLevelsSystem _zLevels = null!;
    [Dependency] private readonly MCZLevelViewSystem _zLevelsView = null!;

    [Dependency] private readonly SharedTransformSystem _transform = null!;

    public bool TryAdjustShotCoordinates(
        EntityUid shooter,
        EntityCoordinates fromCoordinates,
        EntityCoordinates toCoordinates,
        out EntityCoordinates adjustedFromCoordinates,
        out EntityCoordinates adjustedToCoordinates)
    {
        adjustedFromCoordinates = fromCoordinates;
        adjustedToCoordinates = toCoordinates;

        var offset = _zLevelsView.GetRequestedShotOffset(shooter, toCoordinates);
        if (offset == 0)
            return true;

        var fromMap = _transform.ToMapCoordinates(fromCoordinates);
        var toMap = _transform.ToMapCoordinates(toCoordinates);
        var clampedTo = MCZLevelMath.ClampCrossZShotTarget(fromMap.Position, toMap.Position);
        if (!TryGetShotOpening(shooter, offset, fromMap.Position, clampedTo, out var opening))
            return false;

        MCZLevelMath.GetCrossZProjectilePath(
            fromMap.Position,
            toMap.Position,
            clampedTo,
            opening.Position,
            offset,
            out var projectileFrom,
            out var projectileTo);

        var targetFrom = new MapCoordinates(projectileFrom, opening.MapId);
        var targetTo = new MapCoordinates(projectileTo, opening.MapId);

        adjustedFromCoordinates = _transform.ToCoordinates(targetFrom);
        adjustedToCoordinates = _transform.ToCoordinates(targetTo);
        return true;
    }

    /// <summary>
    /// Resolves an ability's launch position and aimed point on the firing plane.
    /// Projectiles that descend during flight only need their upward shots projected.
    /// </summary>
    public bool TryGetAbilityShotCoordinates(EntityUid shooter, EntityCoordinates aim,
        out MapCoordinates origin, out MapCoordinates target, float range = float.PositiveInfinity, bool projectDownward = false)
    {
        origin = _transform.GetMapCoordinates(shooter);
        target = _transform.ToMapCoordinates(aim, logError: false);
        if (origin.MapId == MapId.Nullspace || origin.MapId != target.MapId)
            return false;

        target = new MapCoordinates(MCZLevelMath.ClampShotTarget(origin.Position, target.Position, range), target.MapId);
        var offset = _zLevelsView.GetRequestedShotOffset(shooter, aim);
        if (offset == 0 || offset < 0 && !projectDownward)
            return true;

        var searchEnd = MCZLevelMath.ClampCrossZShotTarget(origin.Position, target.Position);
        if (!TryGetShotOpening(shooter, offset, origin.Position, searchEnd, out var opening))
            return false;

        origin = new MapCoordinates(MCZLevelMath.NudgeOpeningTowardSource(opening.Position, origin.Position), opening.MapId);
        target = new MapCoordinates(target.Position, opening.MapId);
        return true;
    }

    private bool TryGetShotOpening(EntityUid shooter, int offset, Vector2 from, Vector2 searchEnd, out MapCoordinates opening)
    {
        opening = default;
        if (Transform(shooter).MapUid is not { } sourceMap ||
            !_zLevels.TryMapOffset(sourceMap, offset, out var targetMap, out var mapComponent))
            return false;

        if (!_zLevels.TryFindZShotOpening(sourceMap, targetMap, offset, from, searchEnd, out var position,
                preferOpeningAwayFromSource: true,
                maxSourceDistanceFromOpeningEdgeTiles: MCZLevelMath.CrossZOpeningSourceEdgeRangeTiles))
            return false;

        opening = new MapCoordinates(position, mapComponent.MapId);
        return true;
    }

    public bool TryGetProjectileVisualOffset(
        EntityUid shooter,
        EntityCoordinates sourceFromCoordinates,
        EntityCoordinates projectileFromCoordinates,
        EntityCoordinates toCoordinates,
        out Vector2 visualOffset)
    {
        visualOffset = default;

        var offset = _zLevelsView.GetRequestedShotOffset(shooter, toCoordinates);
        if (offset == 0)
            return false;

        var sourceFromMap = _transform.ToMapCoordinates(sourceFromCoordinates);
        var projectileFromMap = _transform.ToMapCoordinates(projectileFromCoordinates);

        if (sourceFromMap.MapId == MapId.Nullspace || projectileFromMap.MapId == MapId.Nullspace)
            return false;

        return TryGetProjectileVisualOffset(
            shooter,
            sourceFromMap,
            projectileFromMap,
            toCoordinates,
            out visualOffset);
    }

    public bool TryGetProjectileVisualOffset(
        EntityUid shooter,
        MapCoordinates sourceFromCoordinates,
        MapCoordinates projectileFromCoordinates,
        EntityCoordinates toCoordinates,
        out Vector2 visualOffset)
    {
        visualOffset = default;

        var offset = _zLevelsView.GetRequestedShotOffset(shooter, toCoordinates);
        if (offset == 0)
            return false;

        if (sourceFromCoordinates.MapId == MapId.Nullspace || projectileFromCoordinates.MapId == MapId.Nullspace)
            return false;

        // Keep the projectile physics on the opening path, but shift its sprite to
        // the barrel position in the compensated Z render pass.
        visualOffset = sourceFromCoordinates.Position - MCZLevelMath.GetCrossZRenderOffset(offset) - projectileFromCoordinates.Position;
        return visualOffset.LengthSquared() > 0.001f;
    }

    public void ApplyProjectileVisualOffset(List<EntityUid>? projectiles, Vector2 visualOffset)
    {
        if (projectiles is null || visualOffset.LengthSquared() <= 0.001f)
            return;

        foreach (var projectile in projectiles)
        {
            ApplyProjectileVisualOffset(projectile, visualOffset);
        }
    }

    public void ApplyProjectileVisualOffset(EntityUid projectile, Vector2 visualOffset)
    {
        if (visualOffset.LengthSquared() <= 0.001f)
            return;

        // Do not dirty server-owned entities during client prediction. Server state
        // will add the synced visual offset when the shot is confirmed.
        if (_timing.InPrediction && !IsClientSide(projectile))
        {
            if (!TryComp<MCZLevelPredictedProjectileVisualOffsetComponent>(projectile, out var predictedVisual))
            {
                predictedVisual = new MCZLevelPredictedProjectileVisualOffsetComponent
                {
                    Offset = visualOffset,
                };

                AddComp(projectile, predictedVisual);
                return;
            }

            predictedVisual.Offset = visualOffset;
            return;
        }

        if (!TryComp<MCZLevelProjectileVisualOffsetComponent>(projectile, out var visual))
        {
            visual = new MCZLevelProjectileVisualOffsetComponent
            {
                Offset = visualOffset,
            };

            AddComp(projectile, visual);
            Dirty(projectile, visual);
            return;
        }

        visual.Offset = visualOffset;
        Dirty(projectile, visual);
    }
}
