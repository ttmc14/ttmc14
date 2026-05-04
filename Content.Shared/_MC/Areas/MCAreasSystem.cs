using Content.Shared._RMC14.Areas;
using Robust.Shared.Map;

namespace Content.Shared._MC.Areas;

public sealed class MCAreasSystem : EntitySystem
{
    [ViewVariables]
    private static readonly LocId UnknownAreaLocId = "mc-area-unknown";

    [Dependency] private readonly AreaSystem _rmcArea = null!;

    public string GetAreaName(EntityUid coordinates)
    {
        return _rmcArea.TryGetArea(coordinates, out _, out var areaPrototype) ? areaPrototype.Name : Loc.GetString(UnknownAreaLocId);
    }

    public bool GetAreaCoordsMessage(EntityUid coordinates, out Vector2i coords, out string areaName)
    {
        var position = Transform(coordinates).Coordinates;
        coords = new Vector2i((int) position.X, (int) position.Y);
        areaName = GetAreaName(coordinates);
        return true;
    }

    public string GetAreaCoordsMessage(EntityUid coordinates)
    {
        var position = Transform(coordinates).Coordinates;
        var x = (int) position.X;
        var y = (int) position.Y;
        return $"{GetAreaName(coordinates)} (X: {x}, Y: {y})";
    }

    public bool AreaHas<T>(EntityCoordinates coordinates) where T : IComponent
    {
        return _rmcArea.TryGetArea(coordinates, out var area, out _) && HasComp<T>(area.Value);
    }

    public bool AreaHas<T>(MapCoordinates coordinates) where T : IComponent
    {
        return _rmcArea.TryGetArea(coordinates, out var area, out _) && HasComp<T>(area.Value);
    }
}
