using Content.Shared._MC.MarkerEye.Components;
using Content.Shared.Movement.Components;
using Content.Shared.Movement.Systems;

namespace Content.Shared._MC.MarkerEye;

public sealed partial class MCMarkerEyeSystem : EntitySystem
{
    [Dependency] private readonly SharedEyeSystem _eye = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;
    [Dependency] private readonly SharedMoverController _mover = null!;

    public override void Initialize()
    {
        InitializeEntry();

        SubscribeLocalEvent<MCMarkerEyeComponent, ComponentShutdown>(OnEyeShutdown);
        SubscribeLocalEvent<MCMarkerEyeOriginComponent, ComponentShutdown>(OnOriginShutdown);
    }

    private void OnEyeShutdown(Entity<MCMarkerEyeComponent> entity, ref ComponentShutdown args)
    {
        if (entity.Comp.Origin is not { } origin)
            return;

        if (TerminatingOrDeleted(origin))
            return;

        TryStopWatch(origin);
    }

    private void OnOriginShutdown(Entity<MCMarkerEyeOriginComponent> entity, ref ComponentShutdown args)
    {
        CleanupWatch(entity);
    }

    private void CleanupWatch(Entity<MCMarkerEyeOriginComponent> entity)
    {
        var eyeInstance = entity.Comp.Eye;

        _eye.SetTarget(entity, null);
        _eye.SetDrawFov(entity, true);

        _eye.SetZoom(entity, entity.Comp.OriginalZoom);
        _eye.SetPvsScale(entity.Owner, entity.Comp.OriginalPvsScale);

        entity.Comp.Eye = null;

        RemCompDeferred<RelayInputMoverComponent>(entity);

        if (eyeInstance is not { } eye)
            return;

        if (TerminatingOrDeleted(eye))
            return;

        if (TryComp<MCMarkerEyeComponent>(eye, out var eyeComponent))
            eyeComponent.Origin = null;

        PredictedDel(eye);
    }
}
