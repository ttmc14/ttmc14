using Content.Shared._RMC14.Areas;

namespace Content.Shared._MC.Areas;

public sealed class MCAreasSystem : EntitySystem
{
    private static readonly LocId UnknownAreaLocId = "mc-area-unknown";

    [Dependency] private readonly AreaSystem _rmcArea = default!;

    public string GetAreaName(EntityUid coordinates)
    {
        return _rmcArea.TryGetArea(coordinates, out _, out var areaPrototype) ? areaPrototype.Name : Loc.GetString(UnknownAreaLocId);
    }

    public string GetAreaCoordsMessage(EntityUid coordinates)
    {
        var position = Transform(coordinates).Coordinates;
        var x = (int) position.X;
        var y = (int) position.Y;
        return $"{GetAreaName(coordinates)} (X: {x}, Y: {y})";
    }
}
