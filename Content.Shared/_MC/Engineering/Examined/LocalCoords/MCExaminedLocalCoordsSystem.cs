using Content.Shared._MC.Areas;
using Content.Shared.Examine;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Engineering.Examined.LocalCoords;

public sealed class MCExaminedLocalCoordsSystem : EntitySystem
{
    [Dependency] private readonly MCAreasSystem _mcArea = null!;

    public override void Initialize()
    {
        SubscribeLocalEvent<MCExaminedLocalCoordsComponent, ExaminedEvent>(OnExamined);
    }

    private void OnExamined(Entity<MCExaminedLocalCoordsComponent> entity, ref ExaminedEvent args)
    {
        _mcArea.GetAreaCoordsMessage(entity, out var coordinates, out var areaName);

        var message = new FormattedMessage();
        message.AddMarkupOrThrow(Loc.GetString("mc-examined-local-coords",
            ("x", coordinates.X),
            ("y", coordinates.Y),
            ("area", areaName)
        ));

        args.PushMessage(message);
    }
}
