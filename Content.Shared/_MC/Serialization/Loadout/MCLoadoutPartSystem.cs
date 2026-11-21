using Content.Shared._MC.Serialization.Loadout.Data;

namespace Content.Shared._MC.Serialization.Loadout;

public abstract class MCLoadoutPartSystem : EntitySystem
{
    internal abstract MCLoadoutPart? CapturePart(EntityUid uid);
    internal abstract void RestorePart(EntityUid uid, MCLoadoutPart part, MCLoadoutRestoreContext context);
}

/// <summary>
/// Implement Capture and Restore to include a component in loadouts.
/// </summary>
public abstract class MCLoadoutPartSystem<TComponent, TPart> : MCLoadoutPartSystem
    where TComponent : Component
    where TPart : MCLoadoutPart
{
    [Dependency] protected readonly MCLoadoutSerializerSystem Serializer = null!;

    public override void Initialize() =>
        Serializer.Register(this);

    internal sealed override MCLoadoutPart? CapturePart(EntityUid uid) =>
        TryComp<TComponent>(uid, out var component) ? Capture((uid, component)) : null;

    internal sealed override void RestorePart(EntityUid uid, MCLoadoutPart part, MCLoadoutRestoreContext context)
    {
        if (TryComp<TComponent>(uid, out var component))
            Restore((uid, component), (TPart) part, context);
    }

    protected abstract TPart? Capture(Entity<TComponent> entity);

    protected abstract void Restore(Entity<TComponent> entity, TPart part, MCLoadoutRestoreContext context);
}
