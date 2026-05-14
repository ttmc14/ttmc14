using Content.Shared._CE.ZLevels.Core.Components;
using Robust.Shared.Physics.Events;

namespace Content.Shared._CE.ZLevels.Core.EntitySystems;

public abstract partial class CESharedZLevelsSystem
{
    private readonly List<EntityUid> _activeBodies = new();

    public IReadOnlyList<EntityUid> ActiveBodies => _activeBodies;

    private void InitializeActivation()
    {
        SubscribeLocalEvent<CEZPhysicsComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CEZPhysicsComponent, ComponentShutdown>(OnShutdown);

        SubscribeLocalEvent<CEZPhysicsComponent, AnchorStateChangedEvent>(OnAnchorStateChanged);
        SubscribeLocalEvent<CEZPhysicsComponent, PhysicsBodyTypeChangedEvent>(OnPhysicsBodyTypeChanged);

        SubscribeLocalEvent<CEZPhysicsComponent, EntParentChangedMessage>(OnParentChanged);
    }

    private void OnMapInit(Entity<CEZPhysicsComponent> entity, ref MapInitEvent args)
    {
        RefreshBody(entity);

        var mapUid = Transform(entity).MapUid;

        if (!_zMapQuery.TryComp(mapUid, out var zLevel))
            return;

        if (entity.Comp.CurrentZLevel == zLevel.Depth)
            return;

        entity.Comp.CurrentZLevel = zLevel.Depth;
        DirtyField(entity, entity.Comp, nameof(CEZPhysicsComponent.CurrentZLevel));
    }

    private void OnShutdown(Entity<CEZPhysicsComponent> entity, ref ComponentShutdown args)
    {
        SleepBody((entity, entity));
    }

    private void OnAnchorStateChanged(Entity<CEZPhysicsComponent> entity, ref AnchorStateChangedEvent args)
    {
        RefreshBody(entity);
    }

    private void OnPhysicsBodyTypeChanged(Entity<CEZPhysicsComponent> entity, ref PhysicsBodyTypeChangedEvent args)
    {
        RefreshBody(entity);
    }

    private void OnParentChanged(Entity<CEZPhysicsComponent> entity, ref EntParentChangedMessage args)
    {
        RefreshBody(entity);

        if (!ZPhysicsQuery.TryComp(args.OldParent, out var oldParentPhysics))
            return;

        SetZPosition((entity, entity), oldParentPhysics.LocalPosition);
    }
}
