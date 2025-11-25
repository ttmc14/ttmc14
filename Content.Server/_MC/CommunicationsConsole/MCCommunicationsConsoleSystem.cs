using System.Linq;
using Content.Shared._MC.CommunicationsConsole;
using Content.Server.Shuttles.Components;
using Content.Server.Shuttles.Systems;
using Content.Shared._MC.CommunicationsConsole.Components;
using Content.Shared._MC.CommunicationsConsole.UI;
using Content.Shared.DoAfter;
using Content.Shared._RMC14.Marines.Announce;
using Robust.Shared.EntitySerialization.Systems;
using Robust.Shared.Timing;
using Robust.Shared.Map;
using Robust.Shared.Prototypes;
using Robust.Shared.Random;
using Robust.Shared.Utility;
namespace Content.Server._MC.CommunicationsConsole;

public sealed class MCCommunicationsConsoleSystem : MCSharedCommunicationsConsoleSystem
{
    [Dependency] private readonly ShuttleSystem _shuttle = default!;
    [Dependency] private readonly IRobustRandom _random = default!;
    [Dependency] private readonly IGameTiming _timing = default!;
    [Dependency] private readonly SharedMarineAnnounceSystem _marineAnnounce = default!;
    [Dependency] private readonly SharedTransformSystem _transform = default!;
    [Dependency] private readonly SharedDoAfterSystem _doAfter = default!;
    [Dependency] private readonly SharedMapSystem _mapSystem = default!;
    [Dependency] private readonly SharedUserInterfaceSystem _userInterface = default!;
    [Dependency] private readonly IMapManager _mapManager = default!;
    [Dependency] private readonly IPrototypeManager _prototypeManager = default!;

    protected override void OnRunMessage(Entity<MCCommunicationsConsoleComponent> entity, ref MCCommunicationsConsoleERTCallBuiMessage args)
    {
        if (entity.Comp.ERTCalled)
            return;

        entity.Comp.ERTCalled = true;
        Dirty(entity);

        _marineAnnounce.AnnounceHighCommand(Loc.GetString("ert-announce-text"), Loc.GetString("ert-announce-author"));

        SpawnERTMap(entity.Comp.MapPaths);
        CrashERTShuttle(entity.Comp.FTLFlyTime);
    }
    private void CrashERTShuttle(TimeSpan flyTime)
    {
        var points = EntityQuery<MCERTCrashMarkerComponent>().ToList();
        if (points.Count == 0)
            return;

        var point = _random.Pick(points);
        var pointUid = point.Owner;

        if (!TryComp<MCERTCrashMarkerComponent>(pointUid, out var crashMarker))
            return;

        var query = EntityQueryEnumerator<MCERTShuttleComponent, ShuttleComponent>();
        while (query.MoveNext(out var uid, out var ertShuttle, out var shuttle))
        {
            _shuttle.FTLToCoordinates(
                uid,
                shuttle,
                Transform(pointUid).Coordinates.Offset(crashMarker.Offset),
                Angle.Zero,
                hyperspaceTime: (float) flyTime.TotalSeconds
            );
            return;
        }
    }

    private void SpawnERTMap(List<ResPath> mapPath)
    {
        var random = new Random();

        var selectedMapPath = mapPath[random.Next(mapPath.Count)];

        var mapId = new MapId(random.Next(1, 1000));

        var mapLoader = EntitySystem.Get<MapLoaderSystem>();
        mapLoader.TryLoadMapWithId(mapId, selectedMapPath, out var map, out _);
        _mapSystem.InitializeMap(mapId);
    }
}
