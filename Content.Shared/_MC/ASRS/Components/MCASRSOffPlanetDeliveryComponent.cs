using Robust.Shared.GameStates;

namespace Content.Shared._MC.ASRS.Components;

/// <summary>
/// Allows ASRS to deliver to this supply beacon even when it is off planet.
/// Used by the Canterbury's permanent cargo destination.
/// </summary>
[RegisterComponent, NetworkedComponent]
public sealed partial class MCASRSOffPlanetDeliveryComponent : Component;
