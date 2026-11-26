using Content.Shared._MC.Engineering.Beacon;
using Content.Shared._CE.ZLevels.Core.Components;
using Content.Shared._CE.ZLevels.Core.EntitySystems;
using Content.Shared._MC._Config;
using Content.Shared._MC.ASRS.Components;
using Content.Shared._MC.Engineering.Beacon.Events;
using Content.Shared._RMC14.Rules;
using Robust.Shared.Configuration;
using Robust.Shared.Map.Components;

namespace Content.Shared._MC.ASRS.Systems;

public sealed partial class MCASRSConsoleSystem
{
    [Dependency] private readonly MCBeaconSystem _mcBeacon = null!;
    [Dependency] private readonly IConfigurationManager _config = null!;
    [Dependency] private readonly CESharedZLevelsSystem _zLevels = null!;

    private bool _refreshDeliveryOptions;
    private bool _allowDeliveryAnywhere;

    private bool IsDeliveryLocation(EntityUid beacon)
    {
        var xform = Transform(beacon);
        if (_allowDeliveryAnywhere || HasComp<MCASRSOffPlanetDeliveryComponent>(beacon) || _planet.IsOnPlanet(xform))
            return true;

        // MC planet maps mark the surface and link the other floors through its Z-level network.
        return xform.MapUid is { } map && TryComp<CEZLevelMapComponent>(map, out var level) &&
            level.Depth != 0 && _zLevels.MapOffset((map, level), -level.Depth) is { } surface &&
            HasComp<RMCPlanetComponent>(surface.Owner);
    }

    private void InitializeBeacon()
    {
        SubscribeLocalEvent<MCBeaconActiveChangedEvent>(OnBeaconActiveChanged);
        SubscribeLocalEvent<MapGridComponent, EntParentChangedMessage>(OnGridParentChanged);

        if (_net.IsServer)
            Subs.CVar(_config, MCConfigVars.AsrsAllowDeliveryAnywhere, OnDeliverySettingChanged, true);
    }

    private void OnDeliverySettingChanged(bool allow)
    {
        _allowDeliveryAnywhere = allow;
        RefreshAll(dirty: false);
    }

    private void OnGridParentChanged(Entity<MapGridComponent> grid, ref EntParentChangedMessage args)
    {
        // A shuttle can carry active beacons onto or off the planet.
        if (_net.IsServer && args.OldMapId != args.Transform.MapUid)
            _refreshDeliveryOptions = true;
    }

    public override void Update(float frameTime)
    {
        if (!_refreshDeliveryOptions)
            return;

        _refreshDeliveryOptions = false;
        RefreshAll(dirty: false);
    }

    private void OnBeaconActiveChanged(ref MCBeaconActiveChangedEvent ev)
    {
        RefreshAll(dirty: false);
    }
}
