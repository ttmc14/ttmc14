using Content.Shared._MC.Shuttles.Space;
using Content.Shared._MC.Shuttles.Space.Components;
using Content.Shared._MC.Shuttles.TargetPoint;
using Content.Shared._MC.Shuttles.TargetPoint.Components;
using Robust.Shared.GameStates;
using Robust.Shared.Utility;

namespace Content.Shared._MC.Shuttles.DropshiPicker.Components;

[RegisterComponent, NetworkedComponent]
public sealed partial class MCDropshipPickerComponent : Component
{
    [DataField]
    public List<MCDropshipPickerEntryData> DropshipGrids = new();

    /// <seealso cref="MCShuttleSpaceSystem"/>
    /// <seealso cref="MCShuttleSpaceComponent"/>
    [DataField]
    public string SpaceCreation = "mc-dropship-picker-creation";

    /// <seealso cref="MCShuttleTargetPointComponent"/>
    /// <seealso cref="MCShuttleTargetPointSystem"/>
    [DataField]
    public string LandPoint = "mc-picker-landpoint";

    [DataField]
    public bool FTL;
}

[DataDefinition, Serializable]
public partial struct MCDropshipPickerEntryData
{
    [DataField]
    public ResPath Path;

    [DataField]
    public string Name;
}
