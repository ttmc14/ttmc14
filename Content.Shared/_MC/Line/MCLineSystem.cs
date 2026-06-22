using Content.Shared._CE.ZLevels.Core;
using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Line;

public sealed class MCLineSystem : EntitySystem
{
    private const float MaxBeamDistance = 500;

    [Dependency] private readonly IComponentFactory _componentFactory = null!;
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly IPrototypeManager _prototype = null!;

    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    public void SpawnEffect(
        EntProtoId spawnId,
        EntityCoordinates start,
        EntityCoordinates end)
    {
        SpawnEffect(spawnId, spawnId, start, end);
    }

    public void SpawnEffect(
        EntProtoId spawnId,
        EntProtoId dataId,
        EntityCoordinates start,
        EntityCoordinates end)
    {
        SpawnEffect(spawnId, dataId, _transform.ToMapCoordinates(start), _transform.ToMapCoordinates(end));
    }

    public void SpawnEffect(
        EntProtoId spawnId,
        MapCoordinates start,
        MapCoordinates end)
    {
        SpawnEffect(spawnId, spawnId, start, end);
    }

    public void SpawnEffect(
        EntProtoId spawnId,
        EntProtoId dataId,
        MapCoordinates start,
        MapCoordinates end)
    {
        if (_net.IsClient)
            return;

        if (start.MapId != end.MapId)
            return;

        if (!_prototype.TryIndex(dataId, out var entityPrototype))
            return;

        var lineComponentName = _componentFactory.GetComponentName<MCLineComponent>();
        if (!entityPrototype.TryGetComponent<MCLineComponent>(lineComponentName, out var component))
            return;

        var delta = end.Position - start.Position;
        var distance = delta.Length();

        if (distance is <= 0f or > MaxBeamDistance)
            return;

        var direction = delta.Normalized();
        var angle = direction.ToAngle();

        var mapUid = _map.GetMap(start.MapId);
        var startCoords = new EntityCoordinates(mapUid, start.Position);

        var sprites = new List<MCLineSpriteData>();

        if (component.Head is not null)
        {
            var coords = startCoords.Offset(direction * 0.5f);
            sprites.Add(new MCLineSpriteData(GetNetCoordinates(coords), angle, component.Head, 1f, spawnId));
        }

        if (component.Body is not null)
        {
            var tileSize = 1f;
            var startOffset = component.Head is not null ? 1f : 0f;
            var endOffset = component.Tail is not null ? distance - 1f : distance;
            var bodyLength = endOffset - startOffset;

            if (bodyLength > 0.01f)
            {
                var count = (int) float.Ceiling(bodyLength / tileSize);
                var step = bodyLength / count;
                var scale = step / tileSize;

                for (var i = 0; i < count; i++)
                {
                    var offset = startOffset + (step * i) + (step * 0.5f);
                    var coords = startCoords.Offset(direction * offset);

                    sprites.Add(new MCLineSpriteData(
                        GetNetCoordinates(coords),
                        angle,
                        component.Body,
                        scale,
                        spawnId));
                }
            }
        }

        if (component.Tail is not null)
        {
            var tailPos = float.Max(0.5f, distance - 0.5f);
            var coords = startCoords.Offset(direction * tailPos);
            sprites.Add(new MCLineSpriteData(GetNetCoordinates(coords), angle.FlipPositive(), component.Tail, 1f, spawnId));
        }

        if (sprites.Count == 0)
            return;

        RaiseNetworkEvent(new MCLineEffectEvent
        {
            Data = sprites,
        }, CEFilter.ZPvs(startCoords));
    }
}
