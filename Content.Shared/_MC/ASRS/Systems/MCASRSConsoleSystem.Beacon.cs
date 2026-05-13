using Content.Shared._MC.Engineering.Beacon;
using Content.Shared._MC.Engineering.Beacon.Events;

namespace Content.Shared._MC.ASRS.Systems;

public sealed partial class MCASRSConsoleSystem
{
    [Dependency] private readonly MCBeaconSystem _mcBeacon = null!;

    private void InitializeBeacon()
    {
        SubscribeLocalEvent<MCBeaconActiveChangedEvent>(OnBeaconActiveChanged);
    }

    private void OnBeaconActiveChanged(ref MCBeaconActiveChangedEvent ev)
    {
        RefreshAll(dirty: false);
    }
}
