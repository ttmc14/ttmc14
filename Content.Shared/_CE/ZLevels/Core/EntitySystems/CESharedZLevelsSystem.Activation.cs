using Content.Shared._CE.ZLevels.Core.Components;
using Robust.Shared.Map.Components;
using Robust.Shared.Physics;
using Robust.Shared.Physics.Components;
using Robust.Shared.Physics.Events;

namespace Content.Shared._CE.ZLevels.Core.EntitySystems;

public abstract partial class CESharedZLevelsSystem
{
    private void InitializeActivation()
    {
        SubscribeLocalEvent<CEZPhysicsComponent, MapInitEvent>(OnMapInit);
        SubscribeLocalEvent<CEZPhysicsComponent, AnchorStateChangedEvent>(OnAnchorStateChange);
        SubscribeLocalEvent<CEZPhysicsComponent, PhysicsBodyTypeChangedEvent>(OnPhysicsBodyTypeChange);
        SubscribeLocalEvent<CEZPhysicsComponent, EntParentChangedMessage>(OnParentChanged);
    }

    private void OnAnchorStateChange(Entity<CEZPhysicsComponent> ent, ref AnchorStateChangedEvent args)
    {
        CheckActivation(ent);
    }

    private void OnMapInit(Entity<CEZPhysicsComponent> ent, ref MapInitEvent args)
    {
        CheckActivation(ent);

        if (!TryComp<CEZLevelMapComponent>(Transform(ent).MapUid, out var zLevelMap))
            return;

        ent.Comp.CurrentZLevel = zLevelMap.Depth;
        DirtyField(ent, ent.Comp, nameof(CEZPhysicsComponent.CurrentZLevel));
    }

    private void OnPhysicsBodyTypeChange(Entity<CEZPhysicsComponent> ent, ref PhysicsBodyTypeChangedEvent args)
    {
        CheckActivation(ent);
    }

    private void OnParentChanged(Entity<CEZPhysicsComponent> ent, ref EntParentChangedMessage args)
    {
        CheckActivation(ent);

        if (ZPhyzQuery.TryComp(args.OldParent, out var oldParentZPhys))
            SetZPosition((ent, ent), oldParentZPhys.LocalPosition);
    }

    private void CheckActivation(Entity<CEZPhysicsComponent> ent)
    {
        if (TerminatingOrDeleted(ent))
            return;

        var xform = Transform(ent);

        if (!HasComp<MapGridComponent>(xform.ParentUid))
        {
            SetActiveStatus(ent, false);
            return;
        }

        if (xform.Anchored)
        {
            SetActiveStatus(ent, false);
            return;
        }

        if (TryComp<PhysicsComponent>(ent, out var physics))
        {
            if (physics.BodyType == BodyType.Static)
            {
                SetActiveStatus(ent, false);
                return;
            }
        }

        SetActiveStatus(ent, true);
    }

    private void SetActiveStatus(EntityUid ent, bool active)
    {
        if (!_timing.IsFirstTimePredicted)
            return;

        if (active)
            EnsureComp<CEActiveZPhysicsComponent>(ent);
        else
            RemComp<CEActiveZPhysicsComponent>(ent);
    }
}
