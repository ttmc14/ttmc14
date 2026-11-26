using Robust.Shared.GameStates;

namespace Content.Shared._MC.Power.Powerpack.Components;

/// <summary>
/// Marks a portable battery that can charge other batteries and power a held laser when worn on the back.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MCPowerpackComponent : Component;
