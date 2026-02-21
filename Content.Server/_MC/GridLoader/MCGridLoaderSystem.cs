using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Robust.Server.GameObjects;
using Robust.Shared.EntitySerialization.Systems;

namespace Content.Server._MC.GridLoader;

public sealed class MCGridLoaderSystem : EntitySystem
{
    [Dependency] private readonly MapLoaderSystem _mapLoader = null!;
    [Dependency] private readonly TransformSystem _transform = null!;
    [Dependency] private readonly ShuttleSystem _shuttle = null!;

    private readonly Queue<(EntityUid Grid, string Key)> _pendingFtl = new();

    public override void Initialize()
    {
        base.Initialize();

        SubscribeLocalEvent<MCGridLoaderComponent, MapInitEvent>(OnMapInit);
    }

    public override void Update(float frameTime)
    {
        base.Update(frameTime);

        while (_pendingFtl.TryDequeue(out var entry))
        {
            var (gridUid, forceFtlKey) = entry;

            if (!TryComp<ShuttleComponent>(gridUid, out var shuttle))
                continue;

            var query = EntityQueryEnumerator<MCGridLoaderFtlPointComponent>();
            while (query.MoveNext(out var uid, out var component))
            {
                if (component.Key != forceFtlKey)
                    continue;

                var coords = Transform(uid).Coordinates;
                _shuttle.FTLToCoordinates(gridUid, shuttle, coords, Angle.Zero, 0.1f, 0.1f);
                break;
            }
        }
    }

    private void OnMapInit(Entity<MCGridLoaderComponent> entity, ref MapInitEvent args)
    {
        var transform = Transform(entity);
        var position = _transform.GetMapCoordinates(transform).Position;
        var rotation = transform.LocalRotation;

        _mapLoader.TryLoadGrid(transform.MapID, entity.Comp.Map, out var gridUidNullable, offset: position, rot: rotation);

        // Force FTL
        if (entity.Comp.ForceFtlKey is not { } forceFtlKey || gridUidNullable is not { } gridUid)
            return;

        _pendingFtl.Enqueue((gridUid, forceFtlKey));
    }
}
