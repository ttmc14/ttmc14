using Content.Shared._CE.ZLevels.Core.Components;
using JetBrains.Annotations;
using Robust.Shared.Physics;

namespace Content.Shared._CE.ZLevels.Core.EntitySystems;

public abstract partial class CESharedZLevelsSystem
{
    [PublicAPI]
    public void WakeBody(Entity<CEZPhysicsComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        if (_activeBodies.Contains(entity))
            return;

        entity.Comp.Sleeping = false;
        entity.Comp.SleepTimer = 0f;

        _activeBodies.Add(entity);
    }

    [PublicAPI]
    public void SleepBody(Entity<CEZPhysicsComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        entity.Comp.Sleeping = true;
        entity.Comp.SleepTimer = 0f;

        _activeBodies.Remove(entity);
    }

    [PublicAPI]
    public void RefreshBody(Entity<CEZPhysicsComponent> entity)
    {
        if (TerminatingOrDeleted(entity))
        {
            SleepBody((entity, entity));
            return;
        }

        var transform = Transform(entity);
        var parent = transform.ParentUid;

        if (parent != transform.MapUid
            || transform.Anchored
            || _physicsQuery.TryComp(entity, out var physics)
            && physics.BodyType == BodyType.Static)
        {
            SleepBody((entity, entity));
            return;
        }

        WakeBody((entity, entity));
    }
}
