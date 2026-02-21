using Robust.Shared.Map;
using Robust.Shared.Network;
using Robust.Shared.Player;
using Robust.Shared.Prototypes;

namespace Content.Shared._MC.Line;

public sealed class MCLineSystem : EntitySystem
{
    // TODO: Cvar
    private const float MaxBeamDistance = 500;

    [Dependency] private readonly IComponentFactory _componentFactory = null!;
    [Dependency] private readonly INetManager _net = null!;
    [Dependency] private readonly IPrototypeManager _prototype = null!;

    [Dependency] private readonly SharedMapSystem _map = null!;
    [Dependency] private readonly SharedTransformSystem _transform = null!;

    public void SpawnEffect(
        EntProtoId effectId,
        EntityCoordinates start,
        EntityCoordinates end)
    {
        SpawnEffect(effectId, _transform.ToMapCoordinates(start), _transform.ToMapCoordinates(end));
    }

    public void SpawnEffect(
        EntProtoId effectId,
        MapCoordinates start,
        MapCoordinates end)
    {
        if (_net.IsClient)
            return;

        if (start.MapId != end.MapId)
            return;

        if (!_prototype.TryIndex(effectId, out var entityPrototype))
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

        if (component.Head is not null && distance >= 1f)
        {
            var coords = startCoords.Offset(direction / 2f);
            sprites.Add(new MCLineSpriteData(GetNetCoordinates(coords), angle, component.Head, 1f, effectId));
        }

        if (component.Body is not null && distance >= 1f)
        {
            var coords = startCoords.Offset(direction * (distance + 0.5f) / 2f);
            sprites.Add(new MCLineSpriteData(GetNetCoordinates(coords), angle, component.Body, distance - 1.5f, effectId));
        }

        if (component.Tail is not null)
        {
            var coords = startCoords.Offset(direction * distance);
            sprites.Add(new MCLineSpriteData(GetNetCoordinates(coords), angle.FlipPositive(), component.Tail, 1f, effectId));
        }

        if (sprites.Count == 0)
            return;

        RaiseNetworkEvent(new MCLineEffectEvent
        {
            Data = sprites,
        },
        Filter.Pvs(startCoords));
    }
}
