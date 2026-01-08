namespace Content.Client.PowerCell;

[RegisterComponent]
public sealed partial class PowerCellVisualsComponent : Component
{
    // mc-changes-start
    [DataField]
    public string ChargePrefix = "o";
    // mc-changes-end
}
