using Content.Shared._MC.MarkerEye.Components;
using Content.Shared.Movement.Components;
using JetBrains.Annotations;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.MarkerEye;

public sealed partial class MCMarkerEyeSystem
{
    [PublicAPI]
    public void TryStartWatch(Entity<MCMarkerEyeOriginComponent?> entity, EntProtoId eyePrototype, MapCoordinates? coordinates = null, bool? overrideFov = null, float? overrideScale = null)
    {
        entity.Comp = EnsureComp<MCMarkerEyeOriginComponent>(entity);

        if (entity.Comp.Eye is not null)
            TryStopWatch(entity);

        coordinates ??= _transform.GetMapCoordinates(entity);

        var eye = PredictedSpawnAtPosition(eyePrototype, _transform.ToCoordinates(coordinates.Value));
        var eyeComponent = EnsureComp<MCMarkerEyeComponent>(eye);

        eyeComponent.Origin = entity;
        Dirty(eye, eyeComponent);

        entity.Comp.Eye = eye;

        if (TryComp<EyeComponent>(entity, out var originEyeComponent))
        {
            entity.Comp.OriginalPvsScale = originEyeComponent.PvsScale;
            entity.Comp.OriginalZoom = originEyeComponent.Zoom;

            _eye.SetTarget(entity, eye, originEyeComponent);

            if (overrideFov is not null)
                _eye.SetDrawFov(entity, overrideFov.Value, originEyeComponent);

            if (overrideScale is not null)
                _eye.SetPvsScale((entity, originEyeComponent), overrideScale.Value);
        }

        _mover.SetRelay(entity, eye);

        Dirty(entity);
    }

    [PublicAPI]
    public void TryStopWatch(Entity<MCMarkerEyeOriginComponent?> entity)
    {
        if (!Resolve(entity, ref entity.Comp, false))
            return;

        CleanupWatch((entity, entity.Comp));
        RemCompDeferred<MCMarkerEyeOriginComponent>(entity);
    }
}
