using Robust.Shared.GameStates;

namespace Content.Shared._MC.SerialNumber;

/// <summary>
/// Stores the object's persistent serial number displayed on examination.
/// </summary>
[RegisterComponent, NetworkedComponent, AutoGenerateComponentState]
[Access(typeof(MCSerialNumberSystem))]
public sealed partial class MCSerialNumberComponent : Component
{
    [DataField, AutoNetworkedField]
    public string Number = string.Empty;
}
