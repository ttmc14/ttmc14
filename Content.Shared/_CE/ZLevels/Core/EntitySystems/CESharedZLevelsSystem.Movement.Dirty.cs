using Content.Shared._CE.ZLevels.Core.Components;
using JetBrains.Annotations;

namespace Content.Shared._CE.ZLevels.Core.EntitySystems;

public abstract partial class CESharedZLevelsSystem
{
    private readonly List<EntityUid> _dirtyMovementBodies = new();

    [PublicAPI]
    public void DirtyMovement(Entity<CEZPhysicsComponent?> entity)
    {
        if (_dirtyMovementBodies.Contains(entity))
            return;

        _dirtyMovementBodies.Add(entity);
    }

    private void UpdateDirtyMovement()
    {
        for (var i = _dirtyMovementBodies.Count - 1; i >= 0; i--)
        {
            var uid = _dirtyMovementBodies[i];

            if (!ZPhysicsQuery.TryComp(uid, out var component))
                continue;

            var entity = (uid, component);
            RequestCacheMovement(entity);
            RefreshBody(entity);
        }

        _dirtyMovementBodies.Clear();
    }
}
